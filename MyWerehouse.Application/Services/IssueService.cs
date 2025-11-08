using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Commands.History.CreateHistoryIssue;
using MyWerehouse.Application.Commands.History.CreateHistoryPicking;
using MyWerehouse.Application.Commands.History.CreateOperation;
using MyWerehouse.Application.Commands.Issue.VerifyIssueAfterLoading;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class IssueService : IIssueService
	{
		private readonly IMediator _mediator;

		private readonly IIssueRepo _issueRepo;
		private readonly IMapper _mapper;//
		private readonly WerehouseDbContext _werehouseDbContext;
		//private readonly IHistoryService _historyService;
		private readonly IInventoryService _inventoryService;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPalletService _palletService;
		private readonly IIssueItemRepo _issueItemRepo;
		private readonly IValidator<CreateIssueDTO> _createIssueValidator;
		//private readonly IValidator<UpdateIssueDTO> _updateIssueValidator;//
		private readonly IEventCollector _eventCollector;
		//private readonly List<INotification> _events = new();

		public IssueService(IMediator mediator)
		{
			_mediator = mediator;
		}
		public IssueService(
			IMediator mediator,
			IIssueRepo issueRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			//IHistoryService historyService,
			IInventoryService inventoryService,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IAllocationRepo allocationRepo,
			IPickingPalletRepo pickingPalletRepo,
			IPalletService palletService,
			IIssueItemRepo issueItemRepo,
			IValidator<CreateIssueDTO> createIssueValidator,
			IEventCollector eventCollector)
			//,IValidator<UpdateIssueDTO> updateIssueValidator)
		{
			_mediator = mediator;
			_issueRepo = issueRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			//_historyService = historyService;
			_inventoryService = inventoryService;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_allocationRepo = allocationRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_palletService = palletService;
			_issueItemRepo = issueItemRepo;
			_createIssueValidator = createIssueValidator;
			_eventCollector = eventCollector;
			//_updateIssueValidator = updateIssueValidator;
		}
		public IssueService(
			IIssueRepo issueRepo)
		{
			_issueRepo = issueRepo;
		}
		public async Task<List<IssueResult>> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, DateTime dateToSend)
		{
			var validationResult = _createIssueValidator.Validate(createIssueDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var issue = new Issue(createIssueDTO.ClientId, createIssueDTO.PerformedBy, dateToSend);
			_issueRepo.AddIssue(issue);
			issue.IssueItems = new List<IssueItem>();
			var addedProducts = new List<IssueResult>();
			foreach (var item in createIssueDTO.Items)
			{
				var notAddedProducts = await AddPalletsToIssueByProductAsync(issue, item);
				addedProducts.Add(notAddedProducts);
				var newItem = new IssueItem
				{
					ProductId = item.ProductId,
					Quantity = item.Quantity,
					BestBefore = item.BestBefore,
				};
				issue.IssueItems.Add(newItem);
			}
			if (addedProducts.Any(r => r.Success == false))
			{
				issue.IssueStatus = IssueStatus.NotComplete;
			}
			//_historyService.CreateHistoryIssue(issue);
			await _werehouseDbContext.SaveChangesAsync();
			await _mediator.Publish(new CreateHistoryIssueCommand(issue.Id, createIssueDTO.PerformedBy));//
			return addedProducts;
		}
		public async Task<IssueResult> AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product)// dla jednego rodzaju produktu
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			var totalAvailable = 0;
			try
			{
				var availablePalletsQuery = await _palletService.GetAllAvailablePalletsAsync(product.ProductId, product.BestBefore);
				totalAvailable = await _inventoryService.GetProductCountAsync(product.ProductId, product.BestBefore);
				if (product.Quantity > totalAvailable)
				{
					throw new ProductException(product.ProductId);
				}
				var numberUnitOnPallet = await _productRepo.GetProductByIdAsync(product.ProductId);
				//ilość kartonów pełnej palety powinno być, nie potrzebne zabezpieczenie
				if (numberUnitOnPallet == null) { throw new ProductException($"Produkt {product.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt"); }
				var number = numberUnitOnPallet.CartonsPerPallet;
				var amountPallets = product.Quantity / number; //liczba palet
				var rest = product.Quantity % number;           //liczba kartonów
				issue.IssueStatus = IssueStatus.Pending;
				// jeszcze warunek by wybierał najpierw pełne palety - first full pallets
				var palletsToAsign = availablePalletsQuery
					.OrderByDescending(p => p.ProductsOnPallet.First(po => po.Quantity > 0).Quantity)
					.Take(amountPallets)
					.ToList();
				foreach (var pallet in palletsToAsign)// dodanie do zlecenia pełnych palet - adding full pallets
				{
					pallet.IssueId = issue.Id;
					pallet.Status = PalletStatus.InTransit;
				_eventCollector.Add(new CreatePalletOperationCommand(pallet.Id,
				pallet.LocationId,
				ReasonMovement.ToLoad,
				issue.PerformedBy,
				PalletStatus.InTransit,
				null));
					//_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.ToLoad, issue.PerformedBy, PalletStatus.InTransit, null);
					issue.Pallets.Add(pallet);
				}
				List<Allocation> newAllocationFromRest = new List<Allocation>();
				//stworzenie zadania picking dla resztówki jeśli rest < 0 -  making picking for rest
				if (rest > 0)
				{
				newAllocationFromRest =	await AddAllocationToIssueAsync(issue, product.ProductId, rest, product.BestBefore, issue.PerformedBy);// palety do pickingu
					await _werehouseDbContext.SaveChangesAsync();
					foreach (var allocation in newAllocationFromRest)
					{
						_eventCollector.Add(new CreateHistoryPickingCommand(
							allocation.VirtualPalletId,
					allocation.Id,
					issue.PerformedBy,
					PickingStatus.Allocated,
					0));
					}
				}
				//await _werehouseDbContext.SaveChangesAsync();
				foreach( var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, CancellationToken.None);
				}
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();

				_eventCollector.Clear();
				
				return IssueResult.Ok("Towar dołączono do wydania", product.ProductId);
			}
			catch (ProductException expr)
			{
				await transaction.RollbackAsync();
				return IssueResult.Fail(
					expr.Message,
					product.ProductId,
					product.Quantity,
					totalAvailable);
			}
			catch (PalletNotFoundException expal)
			{
				await transaction.RollbackAsync();
				return IssueResult.Fail(
					expal.Message,
					product.ProductId);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas przypisywania palet do zlecenia.", ex.InnerException);
			}
		}
		//pobranie zamówienia do aktualizacji 
		public async Task<UpdateIssueDTO> GetIssueByIdAsync(int numberIssue)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(numberIssue);
			if (issue == null) throw new IssueNotFoundException(numberIssue);

			return new UpdateIssueDTO
			{
				Id = issue.Id,
				ClientId = issue.ClientId,
				PerformedBy = issue.PerformedBy,
				Items = issue.Pallets
				 .SelectMany(p => p.ProductsOnPallet)
				 .Select(prod => new IssueItemDTO { ProductId = prod.ProductId, Quantity = prod.Quantity })
				 .ToList(),
				DateToSend = issue.IssueDateTimeSend
			};

		}
		//aktualizacja/poprawienie zamówienia
		public async Task<List<IssueResult>> UpdateIssueAsync(UpdateIssueDTO issueDTO)
		{
			var resultList = new List<IssueResult>();

			var issue = await _issueRepo.GetIssueByIdAsync(issueDTO.Id); //pobranie wydania
			if (issue == null) throw new IssueNotFoundException(issueDTO.Id);
			issue.PerformedBy = issueDTO.PerformedBy;             //1.2 Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone lub nie zaczęty picking
			if (issue.IssueStatus == IssueStatus.New ||
				issue.IssueStatus == IssueStatus.Pending ||
				issue.IssueStatus == IssueStatus.NotComplete)
			{
				//usuwanie palet z issue
				foreach (var pallet in issue.Pallets.ToList())
				{
					_palletRepo.ClearPalletFromListIssue(pallet);
				}
				issue.Pallets.Clear();

				var allocations = await _allocationRepo.GetAllocationsByIssueIdAsync(issueDTO.Id);
				foreach (var allocation in allocations)
				{
					var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(allocation.VirtualPalletId);
					var pallet = await _palletRepo.GetPalletByIdAsync(virtualPallet.PalletId);
					_allocationRepo.DeleteAllocation(allocation);

					if (virtualPallet.Allocations.Count == 0)
					{
						_pickingPalletRepo.DeleteVirtualPalletPicking(virtualPallet);
						pallet.Status = PalletStatus.Available;
					}
					;
				}
				foreach (var item in issueDTO.Items)
				{
					var result = await AddPalletsToIssueByProductAsync(issue, item);
					resultList.Add(result);
				}
				//_historyService.CreateHistoryIssue(issue);
				await _mediator.Publish(new CreateHistoryIssueCommand(issue.Id, issue.PerformedBy));//
				//await _mediator.Send(new CreateHistoryIssueCommand(issue.Id, issue.PerformedBy));
				return resultList;
			}
			else if (issue.IssueStatus == IssueStatus.ConfirmedToLoad)
			{
				// 3. Dodanie dodatkowego zlecenia do załadunku - czyli dwa zlecenia na załadunek
				// Logika: Oblicz różnicę (nowe - stare). Tylko dodatnie ilości -> nowe zlecenie.
				// Ujemne/zerowe: Błąd (nie można zmniejszyć, bo w realizacji).
				// Komunikat: "Dodatkowe zlecenie na ostatnią chwilę, bo stare jest w toku."
				var newQuantities = new List<IssueItemDTO>();
				var hasNegativeDiff = false;
				var errorMessage = new List<string>();

				foreach (var product in issueDTO.Items)
				{
					var productId = product.ProductId;
					var oldQuantity = await _issueItemRepo.GetQuantityByIssueAndProduct(issue, productId);

					var newQuantity = product.Quantity - oldQuantity;
					if (newQuantity < 0)
					{
						hasNegativeDiff = true;
						errorMessage.Add($"Produkt {productId}: Nie można zmniejszyć z {oldQuantity} do {product.Quantity} (różnica : {newQuantity}). Zlecenie jest już zatwierdzone do załadunku");
						continue;
					}
					if (newQuantity > 0)
					{
						var newItem = new IssueItemDTO
						{
							ProductId = productId,
							Quantity = newQuantity,
							BestBefore = product.BestBefore
						};
						newQuantities.Add(newItem);
					}
				}
				if (hasNegativeDiff)
				{
					return new List<IssueResult>
					{
						IssueResult.Fail(string.Join(";", errorMessage),0)
					};
				}
				if (!newQuantities.Any())
				{
					return new List<IssueResult> { IssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.", 0) };
				}
				var dataForNewIssue = new CreateIssueDTO
				{
					ClientId = issue.ClientId,
					Items = newQuantities,
					PerformedBy = issueDTO.PerformedBy,
				};
				resultList = await CreateNewIssueAsync(dataForNewIssue, issueDTO.DateToSend);
				foreach (var result in resultList)
				{
					if (result.Success)
					{
						result.Message += " (Dodatkowe zlecenie na ostatnią chwilę - stare jest w realizacji).";
					}
				}
				return resultList;
			}
			else
			{
				resultList.Add(IssueResult.Fail(
					$"Nie można zaktualizować zlecenia {issue.Id}, status: {issue.IssueStatus}"));
				return resultList;
			}
		}
		//dopracować
		public async Task DeleteIssueAsync(int issueId)
		{
			var issueToDelete = await _issueRepo.GetIssueByIdAsync(issueId)
			?? throw new IssueNotFoundException(issueId);
			if (!(issueToDelete.IssueStatus == IssueStatus.New ||
				issueToDelete.IssueStatus == IssueStatus.Pending ||
				issueToDelete.IssueStatus == IssueStatus.NotComplete)) throw new IssueNotFoundException($"Nie można skasować zlecenia o numerze {issueId}");
			_issueRepo.DeleteIssue(issueToDelete);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//zweryfikować czy wszystkie produkty zostały zrobione na palety - nie wiem czy taka ręczna walidacja potrzebna
		public async Task<IssueResult> VerifyIssueToLoadAsync(int issueId, string userId)
		{
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(issueId)
						?? throw new IssueNotFoundException(issueId);
				issue.IssueStatus = IssueStatus.ConfirmedToLoad;
				//_historyService.CreateHistoryIssue(issue);
				await _werehouseDbContext.SaveChangesAsync();
				await _mediator.Publish(new CreateHistoryIssueCommand(issueId, userId));//
				return IssueResult.Ok("Wydanie zatwierdzono.", issueId);
			}
			catch (IssueNotFoundException ei)
			{
				return IssueResult.Fail(ei.Message);

			}
		}
		//To jest lista do załadunku dla magazyniera lub dla biura do podmian palet
		public async Task<ListPalletsToLoadDTO> LoadingIssueListAsync(int issueId, string sendedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId)
				?? throw new IssueNotFoundException(issueId);
			//zebrać palety po wysyłki 							 
			return new ListPalletsToLoadDTO
			{
				IssueId = issueId,
				ClientId = issue.ClientId,
				ClientName = issue.Client.Name,
				Pallets = issue.Pallets
				.Where(p => p.Status == PalletStatus.InTransit ||
				p.Status == PalletStatus.InStock ||
				p.Status == PalletStatus.Available ||
				p.Status == PalletStatus.ToIssue)
				.Select(p => new PalletToLoadDTO
				{
					PalletId = p.Id,
					LocationName = (p.Location.Bay + " " + p.Location.Aisle + " " + p.Location.Position + " " + p.Location.Height).ToString(),
					PalletStatus = p.Status,
					LocationId = p.LocationId,
					ProductOnPalletIssue = p.ProductsOnPallet.Select(pp => new ProductOnPalletIssueDTO
					{
						ProductId = pp.ProductId,
						ProductName = pp.Product.Name,
						SKU = pp.Product.SKU,
						BestBefore = pp.BestBefore,
						Quantity = pp.Quantity,
					}).ToList()
				}).OrderBy(p => p.LocationId)
				.ToList()
			};
		}
		//zatwierdzenie pojedynczej palety że jest już załadowana - status
		public async Task MarkAsLoadedAsync(string palletId, string sendedBy)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			if (!(pallet.Status == PalletStatus.ToIssue || pallet.Status == PalletStatus.InTransit || pallet.Status == PalletStatus.Available ||
				pallet.Status == PalletStatus.InStock))
			{ throw new PalletNotFoundException("Paleta nie ma statusu do załadowania"); }
			pallet.Status = PalletStatus.Loaded;
			//_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Loaded, sendedBy, PalletStatus.Loaded, null);
			await _werehouseDbContext.SaveChangesAsync();

			await _mediator.Publish(new CreatePalletOperationCommand(
					pallet.Id,
					pallet.LocationId,
					ReasonMovement.Loaded,
					sendedBy,
					PalletStatus.Loaded,
					null));
		}
		// zamyka biuro a nie magazyn w przypadku gdy np. załadunek sie nie mieści
		public async Task FinishIssueNotCompleted(int issueId, string performedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null) throw new IssueNotFoundException(issueId);
			var palletsReturn = new List<Pallet>();
			foreach (var pallet in issue.Pallets.ToList())
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					pallet.Status = PalletStatus.Available;
					pallet.IssueId = null;
					issue.Pallets.Remove(pallet);
					palletsReturn.Add(pallet);
					//_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Correction, performedBy, PalletStatus.Available, null);
				}
				else
				{
					//_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Loaded, performedBy, PalletStatus.Loaded, null);
					foreach (var product in pallet.ProductsOnPallet)
					{
						await _inventoryService.ChangeProductQuantityAsync(product.ProductId, -product.Quantity);
					}
				}
			}
			issue.IssueStatus = IssueStatus.IsShipped;

			//_historyService.CreateHistoryIssue(issue);
			await _werehouseDbContext.SaveChangesAsync();
			foreach (var pallet in issue.Pallets)
			{
				await _mediator.Publish(new CreatePalletOperationCommand(
						pallet.Id,
						pallet.LocationId,
						ReasonMovement.ToLoad,
						issue.PerformedBy,
						PalletStatus.Loaded,
						null
					));
			}
			foreach (var pallet in palletsReturn)
			{
				await _mediator.Publish(new CreatePalletOperationCommand(
						pallet.Id,
						pallet.LocationId,
						ReasonMovement.Correction,
						issue.PerformedBy,
						PalletStatus.Available,
						null
					));
			}
			await _mediator.Publish(new CreateHistoryIssueCommand(issueId, performedBy));
		}
		//zatwierdzenie zakończenia załadunku przez magazyniera
		public async Task CompletedIssueAsync(int issueId, string confirmedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueNotFoundException(issueId);
			foreach (var pallet in issue.Pallets)
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					throw new IssueNotFoundException("Nie załadowano wszystkich palet.");
				}
			}
			issue.IssueStatus = IssueStatus.IsShipped;
			//_historyService.CreateHistoryIssue(issue);
			await _werehouseDbContext.SaveChangesAsync();
			await _mediator.Publish(new CreateHistoryIssueCommand(issueId, confirmedBy));
		}
		//sprawdzenie załadunku i przerzucenie palet załadowanych do archiwum, zmniejszenie zasobu magazynowego
		public async Task<IssueResult> VerifyIssueAfterLoadingAsync(int issueId, string verifyBy)
		{
			//if (_mediator != null)
			//	return await _mediator.Send(new VerifyIssueAfterLoadingCommand(issueId, verifyBy));
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(issueId)
						?? throw new IssueNotFoundException(issueId);
				issue.PerformedBy = verifyBy;
				if (issue.IssueStatus != IssueStatus.IsShipped) throw new IssueNotFoundException("Nie zakończono załadunku.");
				issue.IssueStatus = IssueStatus.Archived;
				foreach (var pallet in issue.Pallets)
				{
					pallet.Status = PalletStatus.Archived;
					foreach (var product in pallet.ProductsOnPallet)
					{
						await _inventoryService.ChangeProductQuantityAsync(product.ProductId, -product.Quantity);

					}
					//_historyService.CreateOperation(pallet, pallet.LocationId, ReasonMovement.Loaded, verifyBy, PalletStatus.Archived, null);
				}
				//_historyService.CreateHistoryIssue(issue);
				
				
				foreach (var pallet in issue.Pallets)
				{
					await _mediator.Publish(new CreatePalletOperationCommand(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.ToLoad,
							issue.PerformedBy,
							PalletStatus.Archived,
							null
						));
				}
				await _mediator.Publish(new CreateHistoryIssueCommand(issueId, verifyBy));//
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				return IssueResult.Ok("Załadunek zatwierdzony, zasoby uaktulanione.");
			}
			catch (IssueNotFoundException ei)
			{
				await transaction.RollbackAsync();
				return IssueResult.Fail(ei.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return IssueResult.Fail("Wystąpił nieoczenikawy błąd przy weryfikacji");
			}
		}
		//podmiana palety podczas załadunku
		public async Task<IssueResult> ChangePalletDuringLoadingAsync(int issueId, string oldPalletId, string newPalletId, string performedBy)
		{
			try
			{
				if (oldPalletId == newPalletId)
				{
					throw new PalletNotFoundException("Nie można podmienić paletę na tą samą");
				}
				var issue = await _issueRepo.GetIssueByIdAsync(issueId);
				if (issue == null) throw new IssueNotFoundException(issueId);
				var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(oldPalletId);
				var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(newPalletId);
				if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
				{
					throw new PalletNotFoundException("Jedna z podanych palet nie istnieje.");
				}
				if (palletToRemoveFromIssue.IssueId != issueId)
				{
					throw new PalletNotFoundException("Paleta do usunięcia nie należy do zlecenia.");
				}
				if (palletToAddingIssue.IssueId != null ||
					(palletToAddingIssue.Status != PalletStatus.Available &&
					palletToAddingIssue.Status != PalletStatus.InStock))
				{
					throw new PalletNotFoundException("Nowej palety nie można przypisać do zlecenia, błędny status.");
				}
				var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
				var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
				if (productOnOldPallet != productOnNewPallet)
				{
					throw new PalletNotFoundException("Nie można podmienić palet z różnymi produktami.");
				}
				palletToAddingIssue.IssueId = issue.Id;
				palletToAddingIssue.Status = PalletStatus.InTransit;
				issue.Pallets.Add(palletToAddingIssue);

				palletToRemoveFromIssue.IssueId = null;
				palletToRemoveFromIssue.Status = PalletStatus.Available;
				issue.Pallets.Remove(palletToRemoveFromIssue);
				issue.IssueStatus = IssueStatus.ChangingPallet;
				issue.PerformedBy = performedBy;
				//_historyService.CreateHistoryIssue(issue);
				//_historyService.CreateOperation(palletToRemoveFromIssue, palletToRemoveFromIssue.LocationId, ReasonMovement.Correction, performedBy, PalletStatus.Available, null);
				//_historyService.CreateOperation(palletToAddingIssue, palletToAddingIssue.LocationId, ReasonMovement.ToLoad, performedBy, PalletStatus.InTransit, null);

				

				await _mediator.Publish(new CreatePalletOperationCommand(
						palletToRemoveFromIssue.Id,
						palletToRemoveFromIssue.LocationId,
						ReasonMovement.Correction,
						issue.PerformedBy,
						PalletStatus.Available,
						null
					));
				await _mediator.Publish(new CreatePalletOperationCommand(
							palletToAddingIssue.Id,
							palletToAddingIssue.LocationId,
							ReasonMovement.ToLoad,
							issue.PerformedBy,
							PalletStatus.ToIssue,
							null
						));

				await _mediator.Publish(new CreateHistoryIssueCommand(issueId, performedBy));
				await _werehouseDbContext.SaveChangesAsync();
				return IssueResult.Ok("Podmieniono palety.", productOnOldPallet.Value);
			}
			catch (PalletNotFoundException ep)
			{
				return IssueResult.Fail(ep.Message);
			}
			catch (IssueNotFoundException ei)
			{
				return IssueResult.Fail(ei.Message);

			}
			catch (Exception ex)
			{
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return IssueResult.Fail("Operacaja się nie powiodła.");
			}
		}
		// To jest lista palet do zdjęcia dla wózkowego
		public async Task<IssuePalletsWithLocationDTO> PalletsToTakeOffList(int issueId, string userId)
		{
			var list = await _issueRepo.GetPalletByIssueIdAsync(issueId);
			//może tu jakieś potwierdzenie że zweryfikowano do załadunku
			var listToShow = new IssuePalletsWithLocationDTO
			{
				IssueId = issueId,
				PalletList = list
			};
			return listToShow;
		}
		public async Task<List<IssueDTO>> GetIssuesByFiltrAsync(IssueReceiptSearchFilter filter)
		{
			try
			{
				var issues = await _issueRepo.GetIssuesByFilter(filter).ToListAsync();
				return _mapper.Map<List<IssueDTO>>(issues);
			}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Error fetching issues.");
				return new List<IssueDTO>();
			}
		}
		//Metody pomocnicze
		private async Task<List<Allocation>> AddAllocationToIssueAsync(Issue issue, int productId, int quantity, DateOnly bestBefore, string userId)
		{
			
			if (quantity <= 0) return new List<Allocation>();
			var listOfAllocation = new List<Allocation>();
			var virtualPallets = await _pickingPalletRepo.GetVirtualPalletsAsync(productId);
			foreach (var virtualPallet in virtualPallets)
			{
				var alreadyAllocated = virtualPallet.Allocations.Sum(a => a.Quantity);
				var availableOnThisPallet = virtualPallet.IssueInitialQuantity - alreadyAllocated;
				if (availableOnThisPallet <= 0) continue;
				var quantityToTake = Math.Min(quantity, availableOnThisPallet);
				var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToTake);
				_allocationRepo.AddAllocation(newAllocation);
				listOfAllocation.Add(newAllocation);
				//_historyService.CreateHistoryPicking(virtualPallet, newAllocation, userId, PickingStatus.Allocated);
				issue.Allocations.Add(newAllocation);
				quantity -= quantityToTake;
				if (quantity <= 0) break;
			}
			while (quantity > 0)
			{
				var newVirtualPallet = await _palletService.AddPalletToPickingAsync(issue, productId, bestBefore, userId);
				var quantityToTake = Math.Min(quantity, newVirtualPallet.IssueInitialQuantity);
				var newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, quantityToTake);
				_allocationRepo.AddAllocation(newAllocation);
				listOfAllocation.Add(newAllocation);
				//_historyService.CreateHistoryPicking(newVirtualPallet, newAllocation, userId, PickingStatus.Allocated);
				quantity -= quantityToTake;				
			}
			return listOfAllocation;
		}
	}
}