using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Events;

namespace MyWerehouse.Domain.Picking.Models
{
	public class PickingTask : AggregateRoots
	{
		public Guid Id { get; set; } = Guid.NewGuid(); //do zmiany z set na private set
		//public int PickingTaskNumber { get; set; }
		public int? VirtualPalletId { get; set; }
		public VirtualPallet? VirtualPallet { get; set; }
		public Guid IssueId { get; set; }		
		public Issue Issue { get; set; }
		public int IssueNumber { get; set; }//
		public int RequestedQuantity { get; set; }
		public PickingStatus PickingStatus { get; set; }


		public int ProductId { get; set; } //
		public DateOnly? BestBefore {  get; set; }
		public string? PickingPalletId { get; set; }
		public Pallet? PickingPallet { get; set; }

		public DateOnly? PickingDay { get; set; }
		public int PickedQuantity { get; set; }
		public void MarkPicked(string pickingPalletId)
		{
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new InvalidOperationException("PickingTask already picked.");

			if (string.IsNullOrWhiteSpace(pickingPalletId))
				throw new ArgumentException("Picking pallet id is required.");
			//czy dołączyć do Issue?
			PickedQuantity = RequestedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.Picked;
		}
		public void MarkPartiallyPicked(string pickingPalletId, int pickedQuantity)
		{
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new InvalidOperationException("PickingTask already picked.");

			if (string.IsNullOrWhiteSpace(pickingPalletId))
				throw new ArgumentException("Picking pallet id is required.");
			PickedQuantity = pickedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.PickedPartially;
		}
		public void AddHistory(string userId, PickingStatus statusBefore, PickingStatus statusAfter, int quantityPicked)
		{
			this.AddDomainEvent(new CreateHistoryPickingNotification(
				Id,
				//PickingTaskNumber,
				VirtualPallet.PalletId,
				IssueId,
				IssueNumber,
				ProductId,
				RequestedQuantity,
				quantityPicked,
				statusBefore,
				statusAfter,
				userId,
				DateTime.UtcNow));
		}
	}
}
