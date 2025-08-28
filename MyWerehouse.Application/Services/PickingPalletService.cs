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
		//private readonly IAllocationRepo _allocationRepo;

		public PickingPalletService(
			IPickingPalletRepo pickingPalletRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			ILocationRepo locationRepo,
			IPalletRepo palletRepo,
			IIssueRepo issueRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_locationRepo = locationRepo;
			_palletRepo = palletRepo;
			_issueRepo = issueRepo;			
		}

		public async Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd)
		//lista palet dla wózkowego do dostaczenia do strafy pickingu
		{
			var pickingPallets = new List<PickingPalletWithLocationDTO>();
			var palletPicking = await _pickingPalletRepo.GetPickingPalletsByTimeAsync(dateMovedStart, dateMovedEnd);
			foreach (var pallet in palletPicking)
			{
				var palletInWarehosue = await _palletRepo.GetPalletByIdAsync(pallet.PalletId);
				var locationName = await _locationRepo.GetLocationByIdAsync(pallet.LocationId);
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
			return pickingPallets
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
					Issues = clientGroup
						.GroupBy(a => a.IssueId)
						.Select(issueGroup => new IssueForPickingDTO
						{
							IssueId = issueGroup.Key,
							Products = issueGroup
								.GroupBy(a => a.ProductId)
								.Select(prodGroup => new ProductOnPalletPickingDTO
								{
									ProductId = prodGroup.Key,
									Quantity = prodGroup.Sum(x => x.Quantity)
								})
								.OrderBy(p => p.ProductId)
								.ToList()
						})
						.OrderBy(i => i.IssueId)
						.ToList()
				})
				.OrderBy(c => c.ClientIdOut)
				.ToList();
		}
		//Lista ile danego towaru dla danej alokacji 
		public async Task<List<ProductToIssueDTO>> GetListToPicking(DateTime dateIssueStart, DateTime dateIssueEnd)
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


		public async Task<List<AllocationDTO>> ShowTaskToDoAsync(string palletSouceScanned, DateTime pickingDate)
		{
			var palletPickingId = await _pickingPalletRepo.GetPickingPalletIdFromPalletIdAsync(palletSouceScanned);
			var allocations = await _pickingPalletRepo.GetAllocationListAsync(palletPickingId, pickingDate);
			return allocations.Select(allocation => new AllocationDTO
			{
				AllocationId = allocation.Id,
				IssueId = allocation.IssueId,
				SourcePalletId = allocation.PickingPallet.Pallet.Id,
				ProductId = allocation.PickingPallet.Pallet.ProductsOnPallet.FirstOrDefault()?.ProductId ?? 0,
				PickingStatus = allocation.PickingStatus,
				Quantity = allocation.Quantity
			}).ToList();
		}

		public async Task DoPickingAsync(AllocationDTO allocationDTO, string userId)
		{
			var allocationtoChange = await _pickingPalletRepo.GetAllocationAsync(allocationDTO.AllocationId);
			allocationtoChange.PickingStatus = PickingStatus.Picked;
			var issueId = allocationtoChange.IssueId;

			await CreatePalletOrAddNewPalletAsync(issueId, allocationDTO.ProductId, allocationtoChange.Quantity);

			var sourcePallet = await _palletRepo.GetPalletByIdAsync(allocationtoChange.PickingPallet.PalletId);
			sourcePallet.ProductsOnPallet.First().Quantity -= allocationtoChange.Quantity;
			if(sourcePallet.ProductsOnPallet.First().Quantity == 0)
			{
				sourcePallet.Status = PalletStatus.Archived;
			}
			//PalletMovements
			
			await _werehouseDbContext.SaveChangesAsync();
		}

		private async Task CreatePalletOrAddNewPalletAsync(int issueId, int productId, int quantity)
		{
			var filter = new PalletSearchFilter
			{
				IssueId = issueId,
				PalletStatus = PalletStatus.Picking,
			};
			var oldPallet = await _palletRepo.GetPalletsByFilter(filter).ToArrayAsync();
			if (!oldPallet.Any())
			{
				//pokaż komunikat weź nową paletę
				var newIdPallet = await _palletRepo.GetNextPalletIdAsync();
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
							ProductId = productId,
							Quantity = quantity,
							DateAdded = DateTime.UtcNow,
						}
					},
					//Issue = 
					//PalletMovements
				};
				var newPallet = await _palletRepo.AddPalletAsync(pallet);

			}
			else
			{
				var pickingPallet = oldPallet[0];
				var existingProduct = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == productId);

				if (existingProduct != null)
				{
					existingProduct.Quantity += quantity;
					//PalletMovements
				}
				else
				{
					pickingPallet.ProductsOnPallet.Add(new ProductOnPallet
					{
						ProductId = productId,
						Quantity = quantity,
						DateAdded = DateTime.UtcNow,
					});
					//PalletMovements
				}
			}
		}
	}
}









