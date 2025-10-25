using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.HistoryDTO;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class HistoryService : IHistoryService
	{
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IHistoryIssueRepo _historyIssueRepo;
		private readonly IHistoryReceiptRepo _historyReceiptRepo;
		private readonly IHistoryPickingRepo _historyPickingRepo;
		private readonly WerehouseDbContext _werehouseDbContext;//

		private readonly ILocationRepo _locationRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IMapper _mapper;

		public HistoryService(
			IPalletMovementRepo palletMovementRepo,
			IHistoryIssueRepo historyIssueRepo,
			IHistoryReceiptRepo historyReceiptRepo,
			IHistoryPickingRepo historyPickingRepo,
			WerehouseDbContext werehouseDbContext//
			, IPalletRepo palletRepo
			, IMapper mapper
			, ILocationRepo locationRepo)
		{
			_palletMovementRepo = palletMovementRepo;
			_historyIssueRepo = historyIssueRepo;
			_historyReceiptRepo = historyReceiptRepo;
			_historyPickingRepo = historyPickingRepo;
			_werehouseDbContext = werehouseDbContext;//
			_palletRepo = palletRepo;
			_mapper = mapper;
			_locationRepo = locationRepo;
		}
		//Create history
		public void CreateHistoryPicking(VirtualPallet virtualPallet, Allocation allocation, string performedBy, PickingStatus statusBefore, int quantityPicked)
		{
			if (quantityPicked == 0)
			{

				if (allocation.PickingStatus == PickingStatus.Picked)
				{
					quantityPicked = allocation.Quantity;
				}
			}
			var history = new HistoryPicking
			{
				Allocation = allocation,
				//AllocationId = allocation.Id,
				VirtualPallet = virtualPallet,
				//VirtualPalletId = virtualPallet.Id,
				Issue = allocation.Issue,
				//IssueId = allocation.IssueId,
				ProductId = virtualPallet.Pallet.ProductsOnPallet.First().ProductId,
				QuantityAllocated = allocation.Quantity,
				QuantityPicked = quantityPicked,
				StatusBefore = statusBefore,
				StatusAfter = allocation.PickingStatus,
				PerformedBy = performedBy,
				DateTime = DateTime.UtcNow,
			};
			_historyPickingRepo.AddHistoryPicking(history);
		}
		public void CreateHistoryPicking(VirtualPallet virtualPallet, Allocation allocation, string performedBy, PickingStatus statusBefore)
		{
			CreateHistoryPicking(
				virtualPallet,
				allocation,
				performedBy,
				statusBefore,
				allocation.PickingStatus == PickingStatus.Picked ? allocation.Quantity : 0
			);
		}
		public void CreateHistoryIssue(Issue issue)
		{
			var details = (issue.Pallets != null && issue.Pallets.Count > 0) ?

				issue.Pallets.Select(p => new HistoryIssueDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
					LocationSnapShot = $"{p.Location.Bay}-{p.Location.Aisle}-{p.Location.Position}-{p.Location.Height}"
				}).ToList(): new List<HistoryIssueDetail>();

			var items = issue.IssueItems.Select(p => new HistoryIssueItems
			{
				ProductId = p.ProductId,
				Quantity = p.Quantity,
				BestBefore = p.BestBefore,
			}).ToList();

			var history = new HistoryIssue
			{
				Issue = issue,
				StatusAfter = issue.IssueStatus,
				PerformedBy = issue.PerformedBy,
				Details = details.ToList(),
				Items = items,
				DateTime = DateTime.UtcNow,
			};
			_historyIssueRepo.AddHistoryIssue(history);
		}
		public void CreateHistoryReceipt(Receipt receipt)
		{
			var details = (receipt.Pallets != null && receipt.Pallets.Count != 0)
				? receipt.Pallets.Select(p => new HistoryReceiptDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
					LocationSnapShot = $"{p.Location.Bay}-{p.Location.Aisle}-{p.Location.Position}-{p.Location.Height}"
				}).ToList(): new List<HistoryReceiptDetail>();
			
			var history = new HistoryReceipt
			{
				Receipt = receipt,
				ClientId = receipt.ClientId,
				StatusAfter = receipt.ReceiptStatus,
				PerformedBy = receipt.PerformedBy,
				DateTime = DateTime.UtcNow,
				Details = details,
			};
			_historyReceiptRepo.AddHistoryReceipt(history);
		}
		public void CreateHistoryReceipt(Receipt receipt, ReceiptStatus receiptStatus, string userId)
		{
			var details = (receipt.Pallets != null && receipt.Pallets.Count != 0)
				? receipt.Pallets.Select(p => new HistoryReceiptDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
					LocationSnapShot = $"{p.Location.Bay}-{p.Location.Aisle}-{p.Location.Position}-{p.Location.Height}"
				}).ToList() : new List<HistoryReceiptDetail>();
			var history = new HistoryReceipt
			{
				Receipt = receipt,
				ClientId = receipt.ClientId,
				StatusAfter = receiptStatus,
				PerformedBy = userId,
				DateTime = DateTime.UtcNow,
				Details = details
			};
			_historyReceiptRepo.AddHistoryReceipt(history);
		}
		public void CreateMovement(Pallet pallet, Location destinationLocation, ReasonMovement reasonMovement, string userId, PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details)
		{
			if (details == null)
			{
				details = pallet.ProductsOnPallet.Select(p => new PalletMovementDetail
				{
					ProductId = p.ProductId,
					Quantity = p.Quantity,
				}).ToList();
			}
			var movement = new PalletMovement
			{
				//PalletId = pallet.Id,
				Pallet = pallet,
				SourceLocationId = pallet.LocationId,
				SourceLocationSnapShot = $"{pallet.Location.Bay}-{pallet.Location.Aisle}-{pallet.Location.Position}-{pallet.Location.Height}",
				DestinationLocationId = destinationLocation.Id,
				DestinationLocationSnapShot = $"{destinationLocation.Bay}-{destinationLocation.Aisle}-{destinationLocation.Position}-{destinationLocation.Height}",
				Reason = reasonMovement,
				PerformedBy = userId,
				PalletMovementDetails = details.ToList(),
				MovementDate = DateTime.UtcNow,
				PalletStatus = palletStatus
			};
			_palletMovementRepo.AddPalletMovement(movement);
		}
		public void CreateOperation(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details)
		{
			if (details == null)
			{
				details = pallet.ProductsOnPallet.Select(p => new PalletMovementDetail
				{
					ProductId = p.ProductId,
					Quantity = p.Quantity,
				}).ToList();
			}
			//if (pallet.Location == null)
			//{
			//	Location locationFull =await _locationRepo.GetLocationByIdAsync(destinationLocationId);

			//	pallet.Location = locationFull;

			//}
			var movement = new PalletMovement
			{
				//PalletId = pallet.Id,
				Pallet = pallet,
				//SourceLocationId = pallet.LocationId,
				DestinationLocationId = destinationLocationId,
				DestinationLocationSnapShot = $"{pallet.Location.Bay}-{pallet.Location.Aisle}-{pallet.Location.Position}-{pallet.Location.Height}",
				Reason = reasonMovement,
				PerformedBy = userId,
				PalletMovementDetails = details.ToList(),
				MovementDate = DateTime.UtcNow,
				PalletStatus = palletStatus
			};
			_palletMovementRepo.AddPalletMovement(movement);
		}
		public void CreateOperation(Pallet pallet, string userId, PalletStatus newStatus)
		{
			//return
			CreateOperation(
			pallet,
			pallet.LocationId,
			ReasonMovement.Picking,
			userId,
			newStatus,
			null);
		}
		//Read history
		public async Task<PalletHistoryDTO> GetHistoryPalletByIdAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id);
			var history = _mapper.Map<PalletHistoryDTO>(pallet);
			var filter = new PalletMovementSearchFilter { };
			var details = await _palletMovementRepo.GetDataByFilter(filter, id)
				.OrderByDescending(a => a.MovementDate)
			 .ProjectTo<PalletMovementDTO>(_mapper.ConfigurationProvider)
			 .ToListAsync();
			//.ToList();
			foreach (var item in details)
			{
				history.PalletMovementsDTO.Add(item);
			}
			return history;
		}

		public Task<ReceiptHistoryDTO> GetHistoryReceiptByIdAsync(string id)
		{
			throw new NotImplementedException();
		}

		Task<IssueHistoryDTO> IHistoryService.GetHistoryIssueByIdAsync(string id)
		{
			throw new NotImplementedException();
		}
	}
}
// TODO Do Get
//var history = await _context.AllocationHistory
//	.AsNoTracking()
//	.ToListAsync();