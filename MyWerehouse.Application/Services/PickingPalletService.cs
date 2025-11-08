using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Commands.History.CreateHistoryIssue;
using MyWerehouse.Application.Commands.History.CreateHistoryPicking;
using MyWerehouse.Application.Commands.History.CreateOperation;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Application.ViewModels.PickingPalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class PickingPalletService : IPickingPalletService
	{
		private readonly IMediator _mediator;		
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly ILocationRepo _locationRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IIssueRepo _issueRepo;
		//private readonly IHistoryService _historyService;
		private readonly IPalletService _palletService;
		private readonly IEventCollector _eventCollector;

		public PickingPalletService(
			IMediator mediator,
			IPickingPalletRepo pickingPalletRepo,
			IAllocationRepo allocationRepo,
			WerehouseDbContext werehouseDbContext,
			ILocationRepo locationRepo,
			IPalletRepo palletRepo,
			IIssueRepo issueRepo,
			//IHistoryService historyService,
			IPalletService palletService
			, IEventCollector eventCollector)
		{
			_mediator = mediator;
			_pickingPalletRepo = pickingPalletRepo;
			_allocationRepo = allocationRepo;
			_werehouseDbContext = werehouseDbContext;
			_locationRepo = locationRepo;
			_palletRepo = palletRepo;
			_issueRepo = issueRepo;
			//_historyService = historyService;
			_palletService = palletService;
			_eventCollector = eventCollector;
		}

		//Part to read
		//lista palet do zdjęcia przez wózkowego pallet's list for operator
		public async Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd)
		//lista palet dla wózkowego do dostaczenia do strafy pickingu
		{
			var pickingPallets = new List<PickingPalletWithLocationDTO>();
			var palletPicking = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateMovedStart, dateMovedEnd);
			foreach (var pallet in palletPicking)
			{
				//var palletInWarehosue = await _palletRepo.GetPalletByIdAsync(pallet.PalletId);
				var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
				if (locationName == null) throw new InvalidDataException($"Brak lokalizacji {pallet.LocationId} w magazynie");//It's shouldn't heppend
																															  //var addedToPicking = await _pickingPalletRepo.TakeDateAddedToPickingAsync(pallet.Id);
				var addedToPicking = pallet.DateMoved;
				var palletInWarehouseDTO = new PickingPalletWithLocationDTO
				{
					PalletId = pallet.PalletId,
					LocationName = locationName.Bay + " " + locationName.Aisle + " " + locationName.Position + " " + locationName.Height,
					AddedToPicking = addedToPicking
				};
				pickingPallets.Add(palletInWarehouseDTO);
			}
			return pickingPallets;
		}
		//Lista ile danego towaru dla danego zlecenia posegregowane i zgrupowane po kliencie Product's list by issue&client
		public async Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd)
		{
			var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return new List<PickingGuideLineDTO>();
			}
			var allNededIssuesIds = pickingPallets
				.SelectMany(p => p.Allocations)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);
			var issueDictionary = allIssues.ToDictionary(i => i.Id);
			return [.. pickingPallets
				.SelectMany(p => p.Allocations.Select(a => new
				{
					IssueId = a.IssueId,
					Quantity = a.Quantity,
					ProductId = p.Pallet.ProductsOnPallet.First().ProductId,
					ClientIdOut = issueDictionary[a.IssueId].ClientId
				}))
				.GroupBy(x => x.ClientIdOut)
				.Select(clientGroup => new PickingGuideLineDTO
				{
					ClientIdOut = clientGroup.Key,
					Issues = [.. clientGroup
						.GroupBy(a => a.IssueId)
						.Select(issueGroup => new IssueForPickingDTO
						{
							IssueId = issueGroup.Key,
							Products = [.. issueGroup
								.GroupBy(a => a.ProductId)
								.Select(prodGroup => new ProductOnPalletPickingDTO
								{
									ProductId = prodGroup.Key,
									Quantity = prodGroup.Sum(x => x.Quantity)
								})
								.OrderBy(p => p.ProductId)]
						})
						.OrderBy(i => i.IssueId)]
				})
				.OrderBy(c => c.ClientIdOut)];
		}
		//Lista ile danego towaru dla danej alokacji Product's list by allocations
		public async Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd)
		//wytyczne- lista ile jakiego produktu do konkretnego zlecenia - zlecenia na daną chwilę, bierzemy zlecenia z danego okresu/dnia
		// pojedyncze rekordy dla każdej alokacji
		{
			var pickingPallets = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return new List<ProductToIssueDTO>();
			}
			var allNededIssuesIds = pickingPallets
				.SelectMany(p => p.Allocations)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);

			var issueDictionary = allIssues.ToDictionary(i => i.Id);

			var aggregationDictionary = new Dictionary<(int ClientId, int
				IssueId, int product), ProductToIssueDTO>();

			foreach (var pallet in pickingPallets)
			{
				var productOnPallet = pallet.Pallet?.ProductsOnPallet?.FirstOrDefault();
				if (productOnPallet == null) continue;
				var productId = productOnPallet.ProductId;

				var allocations = pallet.Allocations;
				foreach (var allocation in allocations)
				{
					if (!issueDictionary.TryGetValue(allocation.IssueId, out var issue))
					{
						continue;
					}
					var clientId = issue.ClientId;
					var key = (clientId, allocation.IssueId, productId);
					if (aggregationDictionary.TryGetValue(key, out var existingRecord))
					{
						existingRecord.Quantity += allocation.Quantity;
					}
					else
					{
						var productIssue = new ProductToIssueDTO
						{
							ClientIdOut = clientId,
							IssueId = allocation.IssueId,
							ProductId = productId,
							Quantity = allocation.Quantity,
						};
						aggregationDictionary.Add(key, productIssue);
					}
				}
			}
			return aggregationDictionary
					.OrderBy(x => x.Key.ClientId)
						.ThenBy(x => x.Key.IssueId)
							.ThenBy(x => x.Key.product)
					.Select(x => x.Value)
					.ToList();
		}

		//Part to write&read
		//pokaż alokacje dla palety Show allocations to do - scan pallet
		public async Task<List<AllocationDTO>> ShowTaskToDoAsync(string palletSourceScanned, DateTime pickingDate)
		{
			var palletPickingId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletSourceScanned);
			var allocations = await _allocationRepo.GetAllocationListAsync(palletPickingId, pickingDate);
			//mapper??
			return allocations.Select(allocation => new AllocationDTO
			{
				AllocationId = allocation.Id,
				IssueId = allocation.IssueId,
				SourcePalletId = allocation.VirtualPallet.Pallet.Id,
				ProductId = allocation.VirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
				PickingStatus = allocation.PickingStatus,
				RequestedQuantity = allocation.Quantity,
				BestBefore = allocation.VirtualPallet.Pallet.ProductsOnPallet.First().BestBefore
			}).ToList();
		}
		//faktyczne działanie pickingu - zmiany w bazie Do pick - arranging goods
		public async Task<PickingResult> DoPickingAsync(AllocationDTO allocationDTO, string userId)
		{

			//var palletOps = new List<CreatePalletOperationCommand>();
			//var pickingHistoryOps = new List<CreateHistoryPickingCommand>();
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//var historyOperations = new List<HistoryOperation>();
			try
			{
				var newVirtualPallet = new VirtualPallet();
				var newAllocation = new Allocation();
				//var newQuantityToAllocation = 0;
				var allocationToChange = await _allocationRepo.GetAllocationAsync(allocationDTO.AllocationId);
				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(allocationToChange.VirtualPalletId);
				var issueId = allocationToChange.IssueId;
				var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueNotFoundException(issueId);
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(allocationDTO.SourcePalletId)
					?? throw new PalletNotFoundException(allocationDTO.SourcePalletId);
				await ProcessPickingActionAsync(sourcePallet, issue, allocationDTO.ProductId, allocationDTO.PickedQuantity, userId);
				if (allocationDTO.RequestedQuantity == allocationDTO.PickedQuantity)
				{
					allocationToChange.PickingStatus = PickingStatus.Picked;
					//_historyService.CreateHistoryPicking(virtualPallet, allocationToChange, userId, PickingStatus.Picked);
					_eventCollector.Add(new CreateHistoryPickingCommand(
							allocationToChange.VirtualPalletId,
							allocationToChange.Id,
							issue.PerformedBy,
							PickingStatus.Allocated,
							0));
				}
				else if (allocationDTO.RequestedQuantity > allocationDTO.PickedQuantity)
				{
					var newQuantityToAllocation = allocationDTO.RequestedQuantity - allocationDTO.PickedQuantity;
					 
					newVirtualPallet = await _palletService.AddPalletToPickingAsync(issue, allocationDTO.ProductId, allocationDTO.BestBefore, userId);
					//verify if exist VirtualPallet??	
				
					newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, newQuantityToAllocation);
					_allocationRepo.AddAllocation(newAllocation);

					//_historyService.CreateHistoryPicking(newVirtualPallet, newAllocation, userId, PickingStatus.Available);

					_eventCollector.Add(new CreateHistoryPickingCommand(
						newVirtualPallet.Id,
						newAllocation.Id,
						userId,
						PickingStatus.Available,
						0));

					//zablokowanie palety źródłowej bo się nie zgadza stan fizyczny/system
					sourcePallet.Status = PalletStatus.OnHold;					
					//_historyService.CreateOperation(sourcePallet, userId, PalletStatus.OnHold);

					_eventCollector.Add(new CreatePalletOperationCommand(sourcePallet.Id,
						sourcePallet.LocationId,
						ReasonMovement.Correction,
						userId,
						PalletStatus.OnHold,
						null));					
				}
				
				if (issue.IssueStatus == IssueStatus.Pending) { issue.IssueStatus = IssueStatus.InProgress; }
				

				if (allocationDTO.RequestedQuantity == allocationDTO.PickedQuantity)
				{
					await _mediator.Publish(new CreateHistoryPickingCommand(
							virtualPallet.Id,
							allocationToChange.Id,
							issue.PerformedBy,
							PickingStatus.Picked,
							0
						));
				}

				await _werehouseDbContext.SaveChangesAsync();
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, CancellationToken.None);
				}
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				_eventCollector.Clear();

				return PickingResult.Ok("Towar dołączono do zlecenia");
			}
			catch (PalletNotFoundException pnfEx)
			{
				await transaction.RollbackAsync();
				return PickingResult.Fail(pnfEx.Message);
			}
			catch (IssueNotFoundException onfEx)
			{
				await transaction.RollbackAsync();
				return PickingResult.Fail(onfEx.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			}
		}
		public async Task<PickingResult> PrepareManualPickingAsync(string palletId)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			//Nie wyjątek bo to częsta sytuacja w rzeczywistości
			if (pallet == null || pallet.Status == PalletStatus.Archived)
			{
				return PickingResult.Fail($"Brak palety {palletId} na stanie.");
			}

			if (pallet.Status != PalletStatus.ToPicking)
			{
				return PickingResult.Fail($"Paleta {palletId} nie jest w pickingu, zmień status.");
			}

			var product = pallet.ProductsOnPallet.FirstOrDefault();
			if (product == null)
			{
				return PickingResult.Fail($"Paleta {palletId} jest pusta.");
			}
			// Logika wyszukiwania pasujących zleceń			
			var timeFrom = DateTime.UtcNow.AddDays(-1);
			var timeTo = DateTime.UtcNow;
			var allocations = await _allocationRepo.GetAllocationsProductIdAsync(product.ProductId, timeFrom, timeTo);
			var grouped = allocations
				.GroupBy(a => a.IssueId)
				.Select(g => new IssueOptions
				{
					IssueId = g.Key,
					QunatityToDo = g.Sum(a => a.Quantity)
				})
				.ToList();
			return PickingResult.RequiresOrder(
				productInfo: $"{product.PalletId} : {product.Quantity}",
				issueOptions: grouped,
				message: "Podaj numer zamówienia by kontynuować");
		}
		public async Task<PickingResult> ExecuteManualPickingAsync(string palletId, int issueId, string userId)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(palletId)
					?? throw new PalletNotFoundException(palletId);
				var issue = await _issueRepo.GetIssueByIdAsync(issueId)
					?? throw new IssueNotFoundException(issueId);
				var product = pallet.ProductsOnPallet.FirstOrDefault()
					?? throw new InvalidOperationException($"Paleta {palletId} jest pusta.");

				// Oblicz, ile faktycznie można/trzeba skompletować
				var allocationsForIssue = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issueId, product.ProductId);
				var neededQuantity = allocationsForIssue.Where(a => a.PickingStatus == PickingStatus.Allocated).Sum(a => a.Quantity);
				var quantityToPick = Math.Min(neededQuantity, product.Quantity);

				if (quantityToPick <= 0)
				{
					return PickingResult.Fail("Brak zapotrzebowania na ten produkt dla wybranego zlecenia.");
				}

				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletId));
				// *** DODAJ TO: Jeśli VirtualPallet nowa/nie istnieje, zapisz ją TERAZ (wygeneruj Id) ***
				if (virtualPallet == null || virtualPallet.Id == 0)
				{
					virtualPallet = new VirtualPallet
					{
						Pallet = pallet,
						PalletId = pallet.Id,
						DateMoved = DateTime.UtcNow,
						LocationId = pallet.LocationId,
						IssueInitialQuantity = pallet.ProductsOnPallet.First(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
						Allocations = new List<Allocation>()
						// Dodaj inne wymagane pola (np. Status, CreatedAt = DateTime.UtcNow)
					};

					_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
				}
				await ReduceAllocationAsync(issue, product.ProductId, quantityToPick, virtualPallet, userId);

				await ProcessPickingActionAsync(pallet, issue, product.ProductId, quantityToPick, userId);

				// Ta logika jest specyficzna dla manuala (tworzenie nowej alokacji)
				var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToPick);
				_allocationRepo.AddAllocation(newAllocation);

				newAllocation.PickingStatus = PickingStatus.Picked;
				//_historyService.CreateHistoryPicking(virtualPallet, newAllocation, userId, PickingStatus.Allocated);
				_eventCollector.AddDeferred(async () =>
						new CreateHistoryPickingCommand(
							newAllocation.VirtualPalletId,
							newAllocation.Id,
							issue.PerformedBy,
							PickingStatus.Allocated,
							0));
				//_eventCollector.Add(new CreateHistoryPickingCommand(
				//			newAllocation.VirtualPalletId,
				//			newAllocation.Id,
				//			issue.PerformedBy,
				//			PickingStatus.Allocated,
				//			0));

				await _werehouseDbContext.SaveChangesAsync();
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn, CancellationToken.None);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(await factory());
				}

				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				_eventCollector.Clear();
				return PickingResult.Ok("Towar dołączono do zlecenia");
			}
			catch (PalletNotFoundException pnfEx)
			{
				await transaction.RollbackAsync();
				return PickingResult.Fail(pnfEx.Message);
			}
			catch (IssueNotFoundException onfEx)
			{
				await transaction.RollbackAsync();
				return PickingResult.Fail(onfEx.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return PickingResult.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.");
			}
		}
		public async Task<PickingResult> ClosePickingPalletAsync(string palletId, int issueId)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(palletId) ?? throw new PalletNotFoundException(palletId);
				var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueNotFoundException(palletId);
				if (pallet.Status != PalletStatus.Picking) { throw new PalletNotFoundException($"Palety {pallet.Id} nie można zamknąć. "); }
				_pickingPalletRepo.ClosePickingPallet(palletId, issueId);
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				await _mediator.Publish(new CreatePalletOperationCommand(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.Picking,
							issue.PerformedBy,
							PalletStatus.ToIssue,
							null
						));
				await _mediator.Publish(new CreateHistoryIssueCommand(issue.Id, issue.PerformedBy));
				return PickingResult.Ok("Zamknięto paletę");
			}
			catch (PalletNotFoundException exp)
			{
				await transaction.RollbackAsync();
				return PickingResult.Fail(exp.Message);
			}
			catch (IssueNotFoundException exo)
			{
				await transaction.RollbackAsync();
				return PickingResult.Fail(exo.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return PickingResult.Fail("Nastąpił nieoczekiwany błąd");
			}
		}

		// metoda pomocnicza dla Picking - picking helper
		private async Task CreatePalletOrAddToPalletAsync(int issueId, int productId, int quantity, string userId, Pallet sourcePallet)
		{
			var filter = new PalletSearchFilter
			{
				IssueId = issueId,
				PalletStatus = PalletStatus.Picking,
			};
			var oldPallet = await _palletRepo.GetPalletsByFilter(filter).FirstOrDefaultAsync();
			if (oldPallet == null)
			{
				//pokaż komunikat weź nową paletę
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
				var sourcePalletBB = sourcePallet.ProductsOnPallet.Single().BestBefore;
				var pallet = new Pallet
				{
					Id = newIdPallet,
					Status = PalletStatus.Picking,
					IssueId = issueId,
					LocationId = 100100,//lokalizacja że polu pickingu
					DateReceived = DateTime.UtcNow,
					ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet
							{
								PalletId = newIdPallet,
								ProductId = productId,
								Quantity = quantity,
								DateAdded = DateTime.UtcNow,
								BestBefore = sourcePalletBB
							}
						},
				};
				_palletRepo.AddPallet(pallet);
				//_historyService.CreateOperation(pallet, userId, PalletStatus.Picking);
				_eventCollector.Add(new CreatePalletOperationCommand(pallet.Id,
				pallet.LocationId,
				ReasonMovement.Picking,
				userId,
				PalletStatus.Picking,
				null));
			}
			else
			{
				var pickingPallet = oldPallet;
				var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);
				if (existingProduct != null)
				{
					existingProduct.Quantity += quantity;
				}
				else
				{
					pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
					{
						ProductId = productId,
						Quantity = quantity,
						DateAdded = DateTime.UtcNow,
					});
				}
				//_historyService.CreateOperation(oldPallet, userId, PalletStatus.Picking);
				_eventCollector.Add(new CreatePalletOperationCommand(oldPallet.Id,
				oldPallet.LocationId,
				ReasonMovement.Picking,
				userId,
				PalletStatus.Picking,
				null));
			}
		}
		private async Task ProcessPickingActionAsync(Pallet sourcePallet, Issue issue, int productId, int quantityToPick, string userId)
		{
			await CreatePalletOrAddToPalletAsync(issue.Id, productId, quantityToPick, userId, sourcePallet);
			var productOnSourcePallet = sourcePallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == productId)
				?? throw new PalletNotFoundException($"Na palecie {sourcePallet.Id} nie znaleziono produktu o Id : {productId}.");
			productOnSourcePallet.Quantity -= quantityToPick;
			if (productOnSourcePallet.Quantity == 0)
			{
				sourcePallet.Status = PalletStatus.Archived;
				//_historyService.CreateOperation(sourcePallet, sourcePallet.LocationId, ReasonMovement.Picking, userId, PalletStatus.Archived, null);
				_eventCollector.Add(new CreatePalletOperationCommand(sourcePallet.Id,
				sourcePallet.LocationId,
				ReasonMovement.Picking,
				issue.PerformedBy,
				PalletStatus.Archived,
				null));
			}
			else
			{
				_eventCollector.Add(new CreatePalletOperationCommand(sourcePallet.Id,
				sourcePallet.LocationId,
				ReasonMovement.Picking,
				issue.PerformedBy,
				PalletStatus.ToPicking,
				null));
				//_historyService.CreateOperation(sourcePallet, sourcePallet.LocationId, ReasonMovement.Picking, userId, PalletStatus.ToPicking, null);
			}
		}
		private async Task ReduceAllocationAsync(Issue issue, int productId, int quantity, VirtualPallet virtualPallet, string userId)
		{
			var allocations = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issue.Id, productId);
			if (allocations == null) throw new Exception("DB Error");//TODO

			foreach (var allocation in allocations)
			{
				if (quantity <= 0) break;

				if (quantity > 0)
				{
					if (allocation.Quantity > quantity)
					{
						allocation.Quantity -= quantity;
						quantity = 0;
						//_historyService.CreateHistoryPicking(virtualPallet, allocation, userId, PickingStatus.Correction);
						_eventCollector.Add(new CreateHistoryPickingCommand(
							allocation.VirtualPalletId,
							allocation.Id,
							issue.PerformedBy,
							PickingStatus.Allocated,
							0));
					}
					else
					{
						quantity -= allocation.Quantity;
						allocation.Quantity = 0;
						//_historyService.CreateHistoryPicking(virtualPallet, allocation, userId, PickingStatus.Correction);
						_eventCollector.Add(new CreateHistoryPickingCommand(
							allocation.VirtualPalletId,
							allocation.Id,
							issue.PerformedBy,
							PickingStatus.Allocated,
							0));
					}
				}
			}
		}
	}
}









