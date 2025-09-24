using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Services
{
	public class HistoryService : IHistoryService
	{
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IHistoryIssueRepo _historyIssueRepo;
		private readonly IHistoryReceiptRepo _historyReceiptRepo;
		private readonly IHistoryPickingRepo _historyPickingRepo;

		public HistoryService(
			IPalletMovementRepo palletMovementRepo,
			IHistoryIssueRepo historyIssueRepo,
			IHistoryReceiptRepo historyReceiptRepo,
			IHistoryPickingRepo historyPickingRepo)
		{
			_palletMovementRepo = palletMovementRepo;
			_historyIssueRepo = historyIssueRepo;
			_historyReceiptRepo = historyReceiptRepo;
			_historyPickingRepo = historyPickingRepo;
		}

		public async Task CreateHistoryPickingAsync(
			VirtualPallet virtualPallet,
			Allocation allocation, 
			string performedBy, 
			PickingStatus statusBefore, 
			int quantityPicked)
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
				//VirtualPallet = virtualPallet,
				VirtualPalletId = virtualPallet.Id,
				//Issue = allocation.Issue,
				IssueId = allocation.IssueId,
				ProductId = virtualPallet.Pallet.ProductsOnPallet.First().ProductId,
				QuantityAllocated = allocation.Quantity,
				QuantityPicked = quantityPicked,
				StatusBefore = statusBefore,
				StatusAfter = allocation.PickingStatus,
				PerformedBy = performedBy,
				DateTime = DateTime.UtcNow,
			};
			await _historyPickingRepo.AddHistoryPickingAsync(history);
		}
		public async Task CreateHistoryPickingAsync(
			VirtualPallet virtualPallet,
			Allocation allocation,
			string performedBy,
			PickingStatus statusBefore)
		{
			await CreateHistoryPickingAsync(
				virtualPallet,
				allocation,
				performedBy,
				statusBefore,
				allocation.PickingStatus == PickingStatus.Picked ? allocation.Quantity : 0
			);
		}
		public async Task CreateHistoryIssueAsync(Issue issue)
		{			
			var	details = issue.Pallets.Select(p => new HistoryIssueDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
				}).ToList();
			
			var history = new HistoryIssue
			{
				IssueId = issue.Id,				
				StatusAfter = issue.IssueStatus,				
				PerformedBy = issue.PerformedBy,
				Details = details.ToList(),
				DateTime = DateTime.UtcNow,
			};
			await _historyIssueRepo.AddHistoryIssueAsync(history);
		}
		
		public async Task CreateHistoryReceiptAsync(Receipt receipt)
		{			
			var history = new HistoryReceipt
			{
				ReceiptId = receipt.Id,
				ClientId = receipt.ClientId,
				StatusAfter = receipt.ReceiptStatus,
				PerformedBy =receipt.PerformedBy,				
				DateTime = DateTime.UtcNow,
			};
			await _historyReceiptRepo.AddHistoryReceiptAsync(history);
		}

		public async Task CreateHistoryReceiptAsync(Receipt receipt, ReceiptStatus receiptStatus, string userId)
		{
			var history = new HistoryReceipt
			{
				ReceiptId = receipt.Id,
				ClientId = receipt.ClientId,
				StatusAfter = receiptStatus,
				PerformedBy = userId,
				DateTime = DateTime.UtcNow,
			};
			await _historyReceiptRepo.AddHistoryReceiptAsync(history);
		}

		public async Task CreateMovementAsync(Pallet pallet, int destinationLocationId,
			ReasonMovement reasonMovement, string userId,
			PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details)
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
				DestinationLocationId = destinationLocationId,
				Reason = reasonMovement,
				PerformedBy = userId,
				PalletMovementDetails = details.ToList(),
				MovementDate = DateTime.UtcNow,
				PalletStatus = palletStatus
			};
			await _palletMovementRepo.AddPalletMovementAsync(movement);
		}

		public Task CreateMovementAsync(Pallet pallet, string userId, PalletStatus newStatus)
		{
			return CreateMovementAsync(
				pallet,
				pallet.LocationId,
				ReasonMovement.Picking,
				userId,
				newStatus,
				null);
		}		
	}
}
