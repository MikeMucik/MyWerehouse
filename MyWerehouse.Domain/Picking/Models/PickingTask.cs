using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Events;
using MyWerehouse.Domain.Picking.PickingExceptions;

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
			new PickingTask(virtualPalletId, issueId, requestedQuantity, pickingStatus, productId,
				bestBefore, pickingPalletId, pickingDay, pickedQuantity);

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
		//TODO docelowo przejśc na 3 strategie i klasy rozwiązań
		public void Cancel(string userId, int issueNumber)
		{
			var oldStatus = PickingStatus;
			if (PickingStatus == PickingStatus.PickedPartially || PickingStatus == PickingStatus.Picked)
				throw new CannotCancelPickingTaskInCurrentStatusException(Id, IssueId, PickingStatus);
			this.PickingStatus = PickingStatus.Cancelled;			
			AddHistoryPicking(userId, null,null, oldStatus, 0);			
			this.RequestedQuantity = 0;//tu czy przed history raczej tu
		}

		public void SetVirtualPallet(Guid virtualPalletId)//to mogłoby być w virtualPallet
		{
			if (VirtualPalletId != null)
				throw new CannotSetVirtualPalletException(Id);
			this.VirtualPalletId = virtualPalletId;
		}

		public void ReduceQuantity(int quantity, string userId)
		{
			var oldStatus = PickingStatus;
			RequestedQuantity -= quantity;
			PickingStatus = PickingStatus.Correction;
			AddHistoryPicking(userId, null, null, oldStatus, 0);
		}
		// do ujednolicenia MarkPicked MarkPartiallyPicked
		public void MarkPicked(Guid pickingPalletId, string pickingPalletNumber, Guid sourcePalletId, string sourcePalletNumber, string userId)
		{
			var	oldStatus = PickingStatus;
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new CannotMakeOperationForStatusException(Id, PickingStatus);
			if (pickingPalletId == Guid.Empty)
				throw new RequiredPickingPalletException();
			PickedQuantity = RequestedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.Picked;
			AddHistoryPicking(userId,sourcePalletId,sourcePalletNumber, pickingPalletId, pickingPalletNumber, oldStatus, PickedQuantity);
		}
		public void MarkPartiallyPicked(Guid pickingPalletId, string pickingPalletNumber, Guid sourcePalletId, string sourcePalletNumber, int pickedQuantity, string userId)
		{
			var oldStatus = PickingStatus;
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new CannotMakeOperationForStatusException(Id, PickingStatus);
			if (pickingPalletId == Guid.Empty)
				throw new RequiredPickingPalletException();
			PickedQuantity = pickedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.PickedPartially;
			AddHistoryPicking(userId, sourcePalletId, sourcePalletNumber, pickingPalletId, pickingPalletNumber, oldStatus, pickedQuantity);
		}
		public void ChangeToAvailable(string userId, string snapShot)
		{
			var pickingTasks = this.VirtualPallet.PickingTasks;
			if (!(pickingTasks.Any(t => t.PickingStatus == PickingStatus.Allocated)))
			{
				VirtualPallet.Pallet.ChangeStatus(PalletStatus.Available);
				VirtualPallet.Pallet.AddHistory(Histories.Models.ReasonMovement.ReversePicking, userId, snapShot);
			}
		}
		//Różne źródła prawdy dlatego przeciążenie
		public void AddHistoryPicking(string userId, Guid? pickingPalletId, string? pickingPalletNumber, PickingStatus statusBefore, int quantityPicked)// PickingStatus statusAfter,
		{

			this.AddDomainEvent(new CreateHistoryPickingNotification(
				Id,
				VirtualPallet?.PalletId,
				VirtualPallet?.Pallet.PalletNumber,
				pickingPalletId,
				pickingPalletNumber,
				IssueId,
				Issue.IssueNumber,
				ProductId,
				RequestedQuantity,
				quantityPicked,
				statusBefore,
				PickingStatus,
				userId,
				DateTime.UtcNow));
		}
		public void AddHistoryPicking(string userId, Guid? sourcePalletId, string? sourcePalletNumber, Guid? pickingPalletId, string? pickingPalletNumber, PickingStatus statusBefore, int quantityPicked)// PickingStatus statusAfter,
		{

			this.AddDomainEvent(new CreateHistoryPickingNotification(
				Id,
				sourcePalletId,
				sourcePalletNumber,
				pickingPalletId,
				pickingPalletNumber,
				IssueId,
				Issue.IssueNumber,
				ProductId,
				RequestedQuantity,
				quantityPicked,
				statusBefore,
				PickingStatus,
				userId,
				DateTime.UtcNow));
		}
	}
}
