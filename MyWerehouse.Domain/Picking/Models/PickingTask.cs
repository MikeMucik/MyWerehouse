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
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Picking.Models
{
	public class PickingTask : AggregateRoots
	{
		public Guid Id { get; private set; }
		public Guid? VirtualPalletId { get; private set; }
		public VirtualPallet? VirtualPallet { get; private set; }
		public Guid IssueId { get; private set; }
		public Issue Issue { get; private set; } = null!;
		public int RequestedQuantity { get; private set; }
		public PickingStatus PickingStatus { get; private set; }
		public Guid ProductId { get; private set; } 
		public Product Product { get; private set; } = null!;// potrzebne by wyświetlać SKU dla prodktu w DTO
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
				throw new TaskWithOutSourceDomainException();
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
		
		public void Cancel(string userId, DateTime createdAt)
		{
			var oldStatus = PickingStatus;
			if (PickingStatus == PickingStatus.PickedPartially || PickingStatus == PickingStatus.Picked)
				throw new CannotCancelPickingTaskInCurrentStatusDomainException(Id, IssueId, PickingStatus);
			this.PickingStatus = PickingStatus.Cancelled;			
			AddHistoryPicking(userId, null,null, oldStatus, 0, createdAt);			
			this.RequestedQuantity = 0;
		}

		public void SetVirtualPallet(Guid virtualPalletId)
		{
			if (VirtualPalletId != null)
				throw new CannotSetVirtualPalletDomainException(Id);
			this.VirtualPalletId = virtualPalletId;
		}

		public void ReduceQuantity(int quantity, string userId, DateTime createdAt)
		{
			var oldStatus = PickingStatus;
			RequestedQuantity -= quantity;
			PickingStatus = PickingStatus.CorrectionPicking;
			AddHistoryPicking(userId, null, null, oldStatus, 0, createdAt);
		}
		
		public void MarkPicked(Guid pickingPalletId, string pickingPalletNumber, Guid sourcePalletId, string sourcePalletNumber, string userId, DateTime createdAt)
		{
			var	oldStatus = PickingStatus;
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new CannotMakeOperationForStatusDomainException(Id, PickingStatus);
			if (pickingPalletId == Guid.Empty)
				throw new RequiredPickingPalletDomainException();
			PickedQuantity = RequestedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.Picked;
			AddHistoryPicking(userId,sourcePalletId,sourcePalletNumber, pickingPalletId, pickingPalletNumber, oldStatus, PickedQuantity, createdAt);
		}
		public void MarkPartiallyPicked(Guid pickingPalletId, string pickingPalletNumber, Guid sourcePalletId, string sourcePalletNumber, int pickedQuantity, string userId, DateTime createdAt)
		{
			var oldStatus = PickingStatus;
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new CannotMakeOperationForStatusDomainException(Id, PickingStatus);
			if (pickingPalletId == Guid.Empty)
				throw new RequiredPickingPalletDomainException();
			PickedQuantity = pickedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.PickedPartially;
			AddHistoryPicking(userId, sourcePalletId, sourcePalletNumber, pickingPalletId, pickingPalletNumber, oldStatus, pickedQuantity, createdAt);
		}

		// Historia pickingu może pochodzić z różnych źródeł, dlatego przeciążenia przekazują jawne dane palet.
		public void AddHistoryPicking(string userId, Guid? pickingPalletId, string? pickingPalletNumber, PickingStatus statusBefore, int quantityPicked, DateTime createdAt)// PickingStatus statusAfter,
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
				createdAt));
		}
		public void AddHistoryPicking(string userId, Guid? sourcePalletId, string? sourcePalletNumber, Guid? pickingPalletId, string? pickingPalletNumber, PickingStatus statusBefore, int quantityPicked, DateTime createdAt)// PickingStatus statusAfter,
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
				createdAt));
		}
		
	}
}
