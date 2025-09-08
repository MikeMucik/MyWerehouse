using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
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
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly ILocationRepo _locationRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IIssueRepo _issueRepo;		
		private readonly IPalletMovementService _palletMovementService;
		private readonly IPalletService _palletService;

		public PickingPalletService(
			IPickingPalletRepo pickingPalletRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			ILocationRepo locationRepo,
			IPalletRepo palletRepo,
			IIssueRepo issueRepo,			
			IPalletMovementService palletMovementService,
			IPalletService palletService)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_locationRepo = locationRepo;
			_palletRepo = palletRepo;
			_issueRepo = issueRepo;			
			_palletMovementService = palletMovementService;
			_palletService = palletService;
		}

		//Część do odczytu z bazy
		//lista palet do zdjęcia przez wózkowego
		public async Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd)
		//lista palet dla wózkowego do dostaczenia do strafy pickingu
		{
			var pickingPallets = new List<PickingPalletWithLocationDTO>();
			var palletPicking = await _pickingPalletRepo.GetPickingPalletsByTimeAsync(dateMovedStart, dateMovedEnd);
			foreach (var pallet in palletPicking)
			{
				//var palletInWarehosue = await _palletRepo.GetPalletByIdAsync(pallet.PalletId);
				var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
				if (locationName == null) throw new InvalidDataException($"Brak lokalizacji {pallet.LocationId} w magazynie");
				var addedToPicking = await _pickingPalletRepo.TakeDateAddedToPickingAsync(pallet.Id);
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
		//Lista ile danego towaru dla danego zlecenia posegregowane i zgrupowane po kliencie
		public async Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd)
		{
			var pickingPallets = await _pickingPalletRepo.GetPickingPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return new List<PickingGuideLineDTO>();
			}
			var allNededIssuesIds = pickingPallets
				.SelectMany(p => p.Allocation)
				.Select(i => i.IssueId)
				.Distinct()
				.ToList();

			var allIssues = await _issueRepo.GetIssuesByIdsAsync(allNededIssuesIds);
			var issueDictionary = allIssues.ToDictionary(i => i.Id);
			return [.. pickingPallets
				.SelectMany(p => p.Allocation.Select(a => new
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
		//Lista ile danego towaru dla danej alokacji 
		public async Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd)
		//wytyczne- lista ile jakiego produktu do konkretnego zlecenia - zlecenia na daną chwilę, bierzemy zlecenia z danego okresu/dnia
		// pojedyncze rekordy dla każdej alokacji
		{
			var pickingPallets = await _pickingPalletRepo.GetPickingPalletsByTimeAsync(dateIssueStart, dateIssueEnd);
			if (pickingPallets.Count == 0)
			{
				return new List<ProductToIssueDTO>();
			}
			var allNededIssuesIds = pickingPallets
				.SelectMany(p => p.Allocation)
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

				var allocations = pallet.Allocation;
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

		//pokaż alokacje dla palety
		public async Task<List<AllocationDTO>> ShowTaskToDoAsync(string palletSouceScanned, DateTime pickingDate)
		{
			var palletPickingId = await _pickingPalletRepo.GetPickingPalletIdFromPalletIdAsync(palletSouceScanned);
			var allocations = await _pickingPalletRepo.GetAllocationListAsync(palletPickingId, pickingDate);
			//czy tu użyć mapera??
			return allocations.Select(allocation => new AllocationDTO
			{
				AllocationId = allocation.Id,
				IssueId = allocation.IssueId,
				SourcePalletId = allocation.PickingPallet.Pallet.Id,
				ProductId = allocation.PickingPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
				PickingStatus = allocation.PickingStatus,
				RequestedQuantity = allocation.Quantity,
				BestBefore = allocation.PickingPallet.Pallet.ProductsOnPallet.First().BestBefore
			}).ToList();
		}
		//faktyczne działanie pickingu - zmiany w bazie
		public async Task DoPickingAsync(AllocationDTO allocationDTO, string userId)
		{
			var allocationToChange = await _pickingPalletRepo.GetAllocationAsync(allocationDTO.AllocationId);

			var issueId = allocationToChange.IssueId;
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(allocationDTO.SourcePalletId);
		
			if (sourcePallet == null)
			{
				throw new InvalidOperationException($"Paleta o numerze {allocationDTO.SourcePalletId} nie istnieje");
			}
			await CreatePalletOrAddNewPalletAsync(issueId, allocationDTO.ProductId, allocationDTO.PickedQuantity, userId, sourcePallet);
			sourcePallet.ProductsOnPallet.Single().Quantity -= allocationToChange.Quantity;
			if (sourcePallet.ProductsOnPallet.Single().Quantity == 0)
			{
				sourcePallet.Status = PalletStatus.Archived;
				await _palletMovementService.CreateMovementAsync(sourcePallet, sourcePallet.LocationId, ReasonMovement.Picking, userId, PalletStatus.Archived, null);
			}
			else 
			{
				await _palletMovementService.CreateMovementAsync(sourcePallet, sourcePallet.LocationId, ReasonMovement.Picking, userId, PalletStatus.ToPicking, null);
			}
			if (allocationDTO.RequestedQuantity == allocationDTO.PickedQuantity)
			{
				allocationToChange.PickingStatus = PickingStatus.Picked;
			}
			else if (allocationDTO.RequestedQuantity > allocationDTO.PickedQuantity)
			{
				var newQuantityToAllocation = allocationDTO.RequestedQuantity - allocationDTO.PickedQuantity;
				var dateBestBefore = allocationDTO.BestBefore;
				var listSourcePallet = await _palletRepo.GetAvailablePallets(allocationDTO.ProductId, dateBestBefore).ToListAsync();
				var nextSourcePallet = listSourcePallet.First();//pobranie pierwszej pasującej palety rozwinąć żeby można było wybrać z listy

				await _palletService.AddPalletToPickingAsync(issueId, allocationDTO.ProductId, allocationDTO.BestBefore, userId);

				await _pickingPalletRepo.AddPalletToPickingAsync(nextSourcePallet.Id);
				// tu trzeba uprościć
				var pickingPalletId = await _pickingPalletRepo.GetPickingPalletIdFromPalletIdAsync(nextSourcePallet.Id);
				var pickingPalletSource = await _pickingPalletRepo.GetPickingPalletByIdAsync(pickingPalletId);
				await _pickingPalletRepo.AddAllocationAsync(pickingPalletSource, issueId, newQuantityToAllocation);
				//zablokowanie palety źródłowej bo się nie zgadza stan fizyczny/system fizyczny 0 a system więcej niż 0
				sourcePallet.Status = PalletStatus.OnHold;
				await _palletMovementService.CreateMovementAsync(sourcePallet, userId, PalletStatus.OnHold);
			}
			await _werehouseDbContext.SaveChangesAsync();
		}

		private async Task CreatePalletOrAddNewPalletAsync(int issueId, int productId, int quantity, string userId, Pallet sourcePallet)
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
				var newPalletId = await _palletRepo.AddPalletAsync(pallet);
				await _werehouseDbContext.SaveChangesAsync();//potrzebny zapis by paleta była już w bazie inaczej brak palety w systemie błąd niżej
				var newPallet = await _palletRepo.GetPalletByIdAsync(newPalletId);
				if (newPallet == null) throw new InvalidOperationException($"Brak palety {newPalletId} w systemie");				
				await _palletMovementService.CreateMovementAsync(newPallet, userId, PalletStatus.Picking);
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
				await _palletMovementService.CreateMovementAsync(oldPallet, userId, PalletStatus.Picking);				
			}			
		}
	}
}









