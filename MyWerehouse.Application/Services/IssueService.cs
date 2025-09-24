using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Services
{
	public class IssueService : IIssueService
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IHistoryService _historyService;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPalletService _palletService;
		private readonly IValidator<CreateIssueDTO> _createIssueValidator; 

		public IssueService(
			IIssueRepo issueRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IHistoryService historyService,
			IInventoryRepo inventoryRepo,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IPickingPalletRepo pickingPalletRepo,
			IPalletService palletService,
			IValidator<CreateIssueDTO> createIssueValidator)
		{
			_issueRepo = issueRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_historyService = historyService;
			_inventoryRepo = inventoryRepo;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_palletService = palletService;
			_createIssueValidator = createIssueValidator;
		}
		public async Task<int> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, DateTime dateToSend)
		{
			var validationResult = _createIssueValidator.Validate(createIssueDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var issue = _mapper.Map<Issue>(createIssueDTO);
			issue.IssueDateTimeCreate = DateTime.UtcNow;
			issue.IssueStatus = IssueStatus.New;
			issue.IssueDateTimeSend = dateToSend;
			await _issueRepo.AddIssueAsync(issue);

			foreach (var item in createIssueDTO.Items)
			{
				await AddPalletsToIssueByProductAsync(issue, item);
			}
			await _historyService.CreateHistoryIssueAsync(issue);
			await _werehouseDbContext.SaveChangesAsync();
			return issue.Id;
		}
		public async Task AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product)// dla jednego produktu
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();

			try
			{
				//var availablePalletsQuery = await _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore)
				//	.ToListAsync(); //dostępne palety					

				var availablePalletsQuery = _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore); //do testów z Mockami

				var totalAvailable = await _inventoryRepo.GetAvailableQuantityAsync(product.ProductId, product.BestBefore);
				if (product.Quantity > totalAvailable)
				{
					throw new InvalidOperationException($"Brak wystarczającej ilości towaru o id {product.ProductId}.Potrzeba: {product.Quantity}, Dostępne: {totalAvailable}");
				}
				var numberUnitOnPallet = await _productRepo.GetProductByIdAsync(product.ProductId);
				//ilość kartonów pełnej palety
				if (numberUnitOnPallet == null) { throw new InvalidDataException("Produkt nie ma ustawionej ilosci kartonów na paletę."); }
				var number = numberUnitOnPallet.CartonsPerPallet;
				var amountPallets = product.Quantity / number; //liczba palet
				var rest = product.Quantity % number;           //liczba kartonów

				issue.IssueStatus = IssueStatus.InProgress;
							
				// jeszcze warunek by wybierał najpierw pełne palety
				var palletsToAsign = availablePalletsQuery
					.OrderByDescending(p => p.ProductsOnPallet.First(po => po.Quantity > 0).Quantity)
					//.OrderBy(p=>p.ProductsOnPallet.First(p=>p.Quantity == number))
					.Take(amountPallets)
					.ToList();

				foreach (var pallet in palletsToAsign)// dodanie do zlecenia pełnych palet
				{
					pallet.IssueId = issue.Id;
					pallet.Status = PalletStatus.InTransit;
					await _historyService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.ToLoad, issue.PerformedBy,PalletStatus.InTransit, null);
					issue.Pallets.Add(pallet);
				}
				await _werehouseDbContext.SaveChangesAsync();
				await AddAllocationToIssueAsync(issue.Id, product.ProductId, rest, product.BestBefore, issue.PerformedBy);// palety do pickingu
				await transaction.CommitAsync();
			}
			catch (Exception ex)//mam dwa różne rozwiązania
			{
				await transaction.RollbackAsync();
				// Loguj błąd
				throw new InvalidOperationException("Wystąpił błąd podczas przypisywania palet do zlecenia.", ex);
			}
		}
		
		private async Task AddAllocationToIssueAsync(int issueId, int productId, int quantity, DateOnly bestBefore, string userId)
		{
			if (quantity <= 0) return;

			var virtualPallets = await _pickingPalletRepo.GetVirtualPalletsAsync(productId);
			foreach (var virtualPallet in virtualPallets)
			{
				var alreadyAllocated = virtualPallet.Allocation.Sum(a => a.Quantity);
				var availableOnThisPallet = virtualPallet.IssueInitialQuantity - alreadyAllocated;
				if (availableOnThisPallet <= 0) continue;
				var quantityToTake = Math.Min(quantity, availableOnThisPallet);
				await _pickingPalletRepo.AddAllocationAsync(virtualPallet, issueId, quantityToTake);
				quantity -= quantityToTake;
				if (quantity <= 0) break;
			}
			await _werehouseDbContext.SaveChangesAsync();
			if (quantity > 0)
			{
				await _palletService.AddPalletToPickingAsync(issueId, productId, bestBefore, userId);
				await AddAllocationToIssueAsync(issueId, productId, quantity, bestBefore, userId);
			}
		}		
		
		//pobranie zamówienia do spojrzenia lub aktualizacji
		public async Task<IssueToUpdateDTO> GetIssueByIdAsync(int numberIssue)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(numberIssue);
			if (issue == null) throw new InvalidDataException($"Nie ma zamówienia o numerze {numberIssue}");
			var updatingIssue = _mapper.Map<IssueToUpdateDTO>(issue);
			return updatingIssue;
		}
		//asktualizacja/poprawienie zamówienia
		public async Task UpdateIssueAsync(int numberIssue, string perfomedBy, ListProductsOfIssue products, DateTime dateToSend)
		{
			var issueToUpdate = await _issueRepo.GetIssueByIdAsync(numberIssue); //pobranie wydania
			issueToUpdate.PerformedBy = perfomedBy;																	 //1.2 Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone
			if (issueToUpdate.IssueStatus == IssueStatus.New || issueToUpdate.IssueStatus == IssueStatus.InProgress)
			{
				foreach (var pallet in issueToUpdate.Pallets.ToList())
				{
					await _palletRepo.ClearPalletFromListIssueAsync(pallet.Id);
				}
				foreach (var item in products.Values)
				{
					await AddPalletsToIssueByProductAsync(issueToUpdate, item);
				}
				await _historyService.CreateHistoryIssueAsync(issueToUpdate);
			}
			else if (issueToUpdate.IssueStatus == IssueStatus.ConfirmedToLoad)//3. Dodanie dodatkowego zlecenia do załdaunku - czyli dwa zlecenia na załadunek
			{
				var dataForNewIssue = new CreateIssueDTO
				{
					ClientId = issueToUpdate.ClientId,
					Items = products.Values,
					PerformedBy = perfomedBy,
				};
				await CreateNewIssueAsync(dataForNewIssue, dateToSend);
			}
			else throw new InvalidOperationException($"Nie można atkualizować zlecenia o numerze {issueToUpdate.Id}");
		}
		public async Task DeleteIssueAsync(int issueId)
		{
			var issueToDelete = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issueToDelete.IssueStatus != IssueStatus.New) throw new InvalidOperationException($"Nie można wykasować zlecenia o numerze {issueId}");
			await _issueRepo.DeleteIssueAsync(issueId);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//zweryfikować czy wszystkie produkty zostały zrobione na palety - nie wiem czy taka ręczna walidacja potrzebna
		public async Task VerifyIssueToLoadAsync(int issueId, string userId)
		{			
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null) throw new InvalidOperationException($"Brak zlecenia o numerze{issueId}");
			issue.IssueStatus = IssueStatus.ConfirmedToLoad;
			await _historyService.CreateHistoryIssueAsync(issue);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//To jest list do załadunku dla magazyniera 
		public async Task<ListPalletsToLoadDTO> LoadingIssueAsync(int issueId, string sendedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			//zebrać palety po wysyłki 							 
			return new ListPalletsToLoadDTO
			{
				IssueId = issueId,
				ClientId = issue.ClientId,
				ClientName = issue.Client.Name,
				Pallets = issue.Pallets
				.Where(p => p.Status == PalletStatus.InTransit ||
				p.Status == PalletStatus.InStock ||
				p.Status == PalletStatus.Available)
				.Select(p => new PalletToLoadDTO
				{
					PalletId = p.Id,
					LocationName = (p.Location.Bay + " " + p.Location.Aisle + " " + p.Location.Position + " " + p.Location.Height).ToString(),
					PalletStatus = p.Status,
					ProductOnPalletIssue = p.ProductsOnPallet.Select(pp => new ProductOnPalletIssueDTO
					{
						ProductId = pp.Id,
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
			if (!(pallet.Status == PalletStatus.InTransit || pallet.Status == PalletStatus.Available ||
				pallet.Status == PalletStatus.InStock))
			{ throw new InvalidOperationException("Paleta nie ma statusu do załadowania"); }
			pallet.Status = PalletStatus.Loaded;
			await _historyService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Loaded, sendedBy,PalletStatus.Loaded, null);
			await _werehouseDbContext.SaveChangesAsync();
		}
		// zamyka biuro a nie magazyn w przypadku gdy np. załadunek sie nie mieści
		public async Task FinishIssueNotCompleted(int issueId, string performedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null) throw new InvalidOperationException($"Brak zlecenia o numerze{issueId}");
			foreach (var pallet in issue.Pallets.ToList())
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					pallet.Status = PalletStatus.Available;
					pallet.IssueId = null;
					issue.Pallets.Remove(pallet);
					await _historyService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Correction, performedBy, PalletStatus.Available, null);
				}
				else
				{
					await _historyService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Loaded, performedBy,PalletStatus.Loaded, null);
					foreach (var product in pallet.ProductsOnPallet)
					{
						await _inventoryRepo.DecreaseInventoryQuantityAsync(product.ProductId, product.Quantity);
					}
				}
			}
			issue.IssueStatus = IssueStatus.IsShipped;
			
			await _historyService.CreateHistoryIssueAsync(issue);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//zatwierdzenie zakończenia załadunku przez magazyniera
		public async Task CompletedIssueAsync(int issueId, string confirmedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			foreach (var pallet in issue.Pallets)
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					throw new InvalidOperationException("Nie załadowano wszystkich palet.");
				}
			}
			issue.IssueStatus = IssueStatus.IsShipped;
			await _historyService.CreateHistoryIssueAsync(issue);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//sprawdzenie załadunku i przerzucenie palet załadowanych do archiwum, zmniejszenie zasobu magazynowego
		public async Task VerifyIssueAfterLoadingAsync(int issueId, string verifyBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			issue.PerformedBy = verifyBy;
			if (issue.IssueStatus != IssueStatus.IsShipped) throw new InvalidOperationException("Nie zakończono załadunku.");
			issue.IssueStatus = IssueStatus.Archived;
			foreach (var pallet in issue.Pallets)
			{
				pallet.Status = PalletStatus.Archived;
				foreach (var product in pallet.ProductsOnPallet)
				{
					await _inventoryRepo.DecreaseInventoryQuantityAsync(product.ProductId, product.Quantity);
					await _historyService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Loaded, verifyBy, PalletStatus.Archived, null);
				}
			}
			await _historyService.CreateHistoryIssueAsync(issue);
			await _werehouseDbContext.SaveChangesAsync();
		}
		//podmiana palety podczas załadunku
		public async Task ChangePalletDuringLoadingAsync(int issueId, string oldPalletId, string newPalletId, string performedBy)
		{
			if (oldPalletId == newPalletId)
			{
				throw new InvalidOperationException("Nie można podmienić paletę na tą samą");
			}
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null)
			{
				throw new KeyNotFoundException($"Zlecenie o numerze {issueId} nie istnieje");
			}
			var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(oldPalletId);
			var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(newPalletId);
			if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
			{
				throw new KeyNotFoundException("Jedna z podanych palet nie iestnieje.");
			}
			if (palletToRemoveFromIssue.IssueId != issueId)
			{
				throw new InvalidOperationException("Paleta do usunięcia nie należy do zlecenia.");
			}
			if (palletToAddingIssue.IssueId != null ||
				(palletToAddingIssue.Status != PalletStatus.Available &&
				palletToAddingIssue.Status != PalletStatus.InStock))
			{
				throw new InvalidOperationException("Nowej palety nie można przypisać do zlecenia.");
			}
			var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			if (productOnOldPallet != productOnNewPallet)
			{
				throw new InvalidOperationException("Nie można podmienić palet z różnymi produktami.");
			}
			palletToAddingIssue.IssueId = issue.Id;
			palletToAddingIssue.Status = PalletStatus.InTransit;
			issue.Pallets.Add(palletToAddingIssue);

			palletToRemoveFromIssue.IssueId = null;
			palletToRemoveFromIssue.Status = PalletStatus.Available;
			issue.Pallets.Remove(palletToRemoveFromIssue);
			issue.IssueStatus = IssueStatus.ChangingPallet;
			issue.PerformedBy = performedBy;
			await _historyService.CreateHistoryIssueAsync(issue);
			await _historyService.CreateMovementAsync(palletToRemoveFromIssue, palletToRemoveFromIssue.LocationId, ReasonMovement.Correction, performedBy, PalletStatus.Available, null);
			await _historyService.CreateMovementAsync(palletToAddingIssue, palletToAddingIssue.LocationId, ReasonMovement.ToLoad, performedBy, PalletStatus.InTransit, null);

			await _werehouseDbContext.SaveChangesAsync();
		}
		// To jest lista palet do zdjęcia dla wózkowego
		public async Task<IssuePalletsWithLocationDTO> PalletsToTakeOffList(int issueId, string userId)
		{
			var list = await _issueRepo.GetPalletByIssueIdAsync(issueId);
			var listToShow = new IssuePalletsWithLocationDTO
			{
				IssueId = issueId,
				PalletList = list
			};
			return listToShow;
		}
	}
}