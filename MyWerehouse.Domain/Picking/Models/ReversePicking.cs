using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Picking.Events;

namespace MyWerehouse.Domain.Picking.Models
{
	public class ReversePicking : AggregateRoots
	{
		public Guid Id { get; private set; } //= Guid.NewGuid(); //do zmiany z set na private set
		public Guid PickingPalletId { get; private set; }//paleta na której jest towar - kompletacyjna
														 //public required string PickingPalletNumber { get; set; }//paleta na której jest towar - kompletacyjna
		public Guid? SourcePalletId { get; private set; }//paleta źródłowa na nią może wrócić towar 
														 //public string? DestinationPalletId { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
		public Guid? DestinationPalletId { get; private set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
															  //public string? DestinationPalletNumber { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
		public Guid ProductId { get; private set; }
		public DateOnly? BestBefore { get; private set; }
		public int Quantity { get; private set; }
		public ReversePickingStatus Status { get; private set; }
		public Guid PickingTaskId { get; private set; }
		public PickingTask PickingTask { get; private set; }
		public DateOnly DateMade { get; private set; }
		public string UserId { get; private set; }
		private ReversePicking() { }

		private ReversePicking(Guid pickingPalletid, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId)
		{
			Id = Guid.NewGuid();
			PickingPalletId = pickingPalletid;
			SourcePalletId = sourcePalletid;
			ProductId = productId;
			BestBefore = bestBefore;
			Quantity = quantity;
			Status = ReversePickingStatus.Pending;
			PickingTaskId = pickingTaskId;
			DateMade = DateOnly.FromDateTime(DateTime.UtcNow);
			UserId = userId;
		}
		public static ReversePicking Create(Guid pickingPalletId, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId)
			=> new ReversePicking(pickingPalletId, sourcePalletid, productId, bestBefore, quantity, pickingTaskId, userId);
		private ReversePicking(Guid id, Guid pickingPalletid, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId)
		{
			Id = id;
			PickingPalletId = pickingPalletid;
			SourcePalletId = sourcePalletid;
			ProductId = productId;
			BestBefore = bestBefore;
			Quantity = quantity;
			Status = ReversePickingStatus.Pending;
			PickingTaskId = pickingTaskId;
			DateMade = DateOnly.FromDateTime(DateTime.UtcNow);
			UserId = userId;
		}
		public static ReversePicking CreateForSeed(Guid id, Guid pickingPalletId, Guid? sourcePalletid, Guid productId, DateOnly? bestBefore, int quantity, Guid pickingTaskId, string userId)
			=> new ReversePicking(id, pickingPalletId, sourcePalletid, productId, bestBefore, quantity, pickingTaskId, userId);

		public void ChangeStatus(ReversePickingStatus status)
		{
			//invariant!!
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
