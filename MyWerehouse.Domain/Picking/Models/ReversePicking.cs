using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Picking.Events;
using MyWerehouse.Domain.Picking.PickingExceptions;

namespace MyWerehouse.Domain.Picking.Models
{
	public class ReversePicking : AggregateRoots
	{
		public Guid Id { get; private set; } 
		public Guid PickingPalletId { get; private set; }//paleta na której jest towar - kompletacyjna
		public Guid? SourcePalletId { get; private set; }//paleta źródłowa na nią może wrócić towar 					 
		public Guid? DestinationPalletId { get; private set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach															 
		public Guid ProductId { get; private set; }
		public DateOnly? BestBefore { get; private set; }
		public int Quantity { get; private set; }
		public ReversePickingStatus Status { get; private set; }
		public Guid PickingTaskId { get; private set; }
		public PickingTask PickingTask { get; private set; } = null!;
		public DateOnly DateMade { get; private set; }
		public string UserId { get; private set; } = string.Empty;
		private ReversePicking() { }

		private ReversePicking(Guid pickingPalletid, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId, DateOnly createdAt)
		{
			Id = Guid.NewGuid();
			PickingPalletId = pickingPalletid;
			SourcePalletId = sourcePalletid;
			ProductId = productId;
			BestBefore = bestBefore;
			Quantity = quantity;
			Status = ReversePickingStatus.Ongoing;
			PickingTaskId = pickingTaskId;
			DateMade = createdAt;
			UserId = userId;
		}
		public static ReversePicking Create(Guid pickingPalletId, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId, DateOnly createdAt)
			=> new ReversePicking(pickingPalletId, sourcePalletid, productId, bestBefore, quantity, pickingTaskId, userId, createdAt);
		private ReversePicking(Guid id, Guid pickingPalletid, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId, DateOnly createdAt)
		{
			Id = id;
			PickingPalletId = pickingPalletid;
			SourcePalletId = sourcePalletid;
			ProductId = productId;
			BestBefore = bestBefore;
			Quantity = quantity;
			Status = ReversePickingStatus.Ongoing;
			PickingTaskId = pickingTaskId;
			DateMade = createdAt;
			UserId = userId;
		}
		public static ReversePicking CreateForSeed(Guid id, Guid pickingPalletId, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId, DateOnly createdAt)
			=> new ReversePicking(id, pickingPalletId, sourcePalletid, productId, bestBefore, quantity, pickingTaskId, userId, createdAt);

		public void ChangeStatus(ReversePickingStatus status)
		{
			//invariant!!
			if(Status == ReversePickingStatus.Cancelled || Status == ReversePickingStatus.Archaived)
			{
				//throw new CannotMakeOperationForStatusDomainException(Id, Status);
			}
			Status = status;
		}
		public void AddHistory(Guid pickingPalletId, string userId, Guid issueId, int issueNumber, ReversePickingStatus before, ReversePickingStatus after)
		{
			this.AddDomainEvent(new CreateHistoryReversePickingNotification(
				Id,
				PickingPalletId,
				SourcePalletId,
				DestinationPalletId,
				issueId,
				issueNumber,
				ProductId,
				Quantity,
				before,
				after,
				userId
				));
		}
	}
}
