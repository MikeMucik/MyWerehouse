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
		public Guid Id { get; private set; }		
		public Guid? VirtualPalletId { get; private set; }
		public VirtualPallet? VirtualPallet { get; private set; }
		public Guid IssueId { get; private set; }
		public Issue Issue { get; private set; }
		public int RequestedQuantity { get; private set; }
		public PickingStatus PickingStatus { get; private set; }
		public Guid ProductId { get; private set; } //
		public DateOnly? BestBefore { get; private set; }
		public Guid? PickingPalletId { get; private set; }
		public Pallet? PickingPallet { get; private set; }
		public DateOnly? PickingDay { get; private set; }
		public int PickedQuantity { get; private set; }

		private PickingTask() { }

		private PickingTask(Guid? virtualPalletId, Guid issueId, int requestedQuantity,
			PickingStatus pickingStatus, Guid productId, DateOnly? bestBefore, Guid? pickingPalletId,
			DateOnly? pickingDay, int pickedQuantity)
		{
			if (pickingStatus == PickingStatus.Allocated && virtualPalletId == null)
			{
				throw new ArgumentNullException(nameof(pickingStatus));
			}
			Id = Guid.NewGuid();
			VirtualPalletId = virtualPalletId;
			IssueId = issueId;
			RequestedQuantity = requestedQuantity;
			PickingStatus = pickingStatus;
			ProductId = productId;
			BestBefore = bestBefore;
			PickingPalletId = pickingPalletId;
			PickingDay = pickingDay;
			PickedQuantity = pickedQuantity;
		}
		public static PickingTask Create(Guid? virtualPalletId, Guid issueId, int requestedQuantity,
			PickingStatus pickingStatus, Guid productId, DateOnly? bestBefore, Guid? pickingPalletId,
			DateOnly? pickingDay, int pickedQuantity) =>
			new PickingTask(virtualPalletId, issueId, requestedQuantity, pickingStatus, productId, bestBefore, pickingPalletId, pickingDay, pickedQuantity);

		private PickingTask(Guid id, Guid? virtualPalletId, Guid issueId, int requestedQuantity,
			PickingStatus pickingStatus, Guid productId, DateOnly? bestBefore, Guid? pickingPalletId,
			DateOnly? pickingDay, int pickedQuantity)
		{			
			Id = id;
			VirtualPalletId = virtualPalletId;
			IssueId = issueId;
			RequestedQuantity = requestedQuantity;
			PickingStatus = pickingStatus;
			ProductId = productId;
			BestBefore = bestBefore;
			PickingPalletId = pickingPalletId;
			PickingDay = pickingDay;
			PickedQuantity = pickedQuantity;
		}

		public static PickingTask CreateForSeed(Guid id, Guid? virtualPalletId, Guid issueId, int requestedQuantity,
			PickingStatus pickingStatus, Guid productId, DateOnly? bestBefore,
			Guid? pickingPalletId, DateOnly? pickingDay, int pickedQuantity) =>
			new PickingTask(id, virtualPalletId, issueId, requestedQuantity, pickingStatus, productId, bestBefore, pickingPalletId, pickingDay, pickedQuantity);
		
		public void Cancel(string userId,int issueNumber)
		{
			this.PickingStatus = PickingStatus.Cancelled;
			AddHistory(userId, VirtualPallet.PalletId, VirtualPallet.Pallet.PalletNumber,issueNumber, PickingStatus.Allocated, PickingStatus.Cancelled, 0);
			this.RequestedQuantity = 0;
		}
		
		public void SetVirtualPallet(Guid virtualPalletId)
		{
			if (VirtualPalletId != null) throw new InvalidOperationException("Task already have virtualPallet.");
			this.VirtualPalletId = virtualPalletId;
		}
		//public void SetVirtualPalletEntity(VirtualPallet virtualPallet)
		//{
		//	//if (VirtualPalletId != null) throw new InvalidOperationException("Task already have virtualPallet.");
		//	this.VirtualPallet = virtualPallet;
		//}
		public void ReduceQuantity(int quantity)
		{
			RequestedQuantity -= quantity;
		}
		public void MarkPicked(Guid pickingPalletId)
		{
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new InvalidOperationException("PickingTask already picked.");
			if (pickingPalletId == Guid.Empty)
				//if (pickingPalletId == null)
				throw new ArgumentException("Picking pallet id is required.");
			//czy dołączyć do Issue?
			PickedQuantity = RequestedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.Picked;
		}
		public void MarkPartiallyPicked(Guid pickingPalletId, int pickedQuantity)
		{
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new InvalidOperationException("PickingTask already picked.");
			if (pickingPalletId == Guid.Empty)
				//if (pickingPalletId == null)
				throw new ArgumentException("Picking pallet id is required.");
			PickedQuantity = pickedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.PickedPartially;
		}
		public void AddHistory(string userId, Guid palletId, string palletNumber,int issueNumber, PickingStatus statusBefore, PickingStatus statusAfter, int quantityPicked)
		{
			this.AddDomainEvent(new CreateHistoryPickingNotification(
				Id,
				palletId,
				palletNumber,
				IssueId,
				issueNumber,
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
