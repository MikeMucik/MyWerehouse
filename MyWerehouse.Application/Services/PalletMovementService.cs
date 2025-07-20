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
	public class PalletMovementService : IPalletMovementService
	{
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IHistoryIssueRepo _historyIssueRepo;

		public PalletMovementService(
			IPalletMovementRepo palletMovementRepo,
			IHistoryIssueRepo historyIssueRepo)
		{
			_palletMovementRepo = palletMovementRepo;
			_historyIssueRepo = historyIssueRepo;
		}

		public async Task CreateHistoryIssueAsync(Issue issue, IssueStatus status, string userId, IEnumerable<HistoryIssueDetail> details)
		{
			if (details == null)
			{
				details = issue.Pallets.Select(p => new HistoryIssueDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
				}).ToList();
			}
			var history = new HistoryIssue
			{
				IssueId = issue.Id,
				Status = status,
				PerfomedBy = userId,
				Details = details.ToList(),
				DateTime = DateTime.UtcNow,
				
			};
			await _historyIssueRepo.AddHistoryIssueAsync(history);
		}

		//public void CreateMovement(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, IEnumerable<PalletMovementDetail> details = null)
		//{
		//	if (details == null)
		//	{
		//		details = pallet.ProductsOnPallet.Select(p => new PalletMovementDetail
		//		{
		//			ProductId = p.ProductId,
		//			Quantity = p.Quantity,
		//		}).ToList();
		//	}
		//	var movement = new PalletMovement
		//	{
		//		PalletId = pallet.Id,
		//		SourceLocationId = pallet.LocationId,
		//		DestinationLocationId = destinationLocationId,
		//		Reason = reasonMovement,
		//		PerformedBy = userId,
		//		PalletMovementDetails = details.ToList()
		//	};
		//	 _palletMovementRepo.AddPalletMovement(movement);
		//}
		public async Task CreateMovementAsync(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, IEnumerable<PalletMovementDetail> details=null)
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
				PalletId = pallet.Id,
				SourceLocationId = pallet.LocationId,
				DestinationLocationId = destinationLocationId,
				Reason = reasonMovement,
				PerformedBy = userId,
				PalletMovementDetails = details.ToList(),
				MovementDate = DateTime.UtcNow,
			};
			await _palletMovementRepo.AddPalletMovementAsync(movement);			
		}		
	}
}
