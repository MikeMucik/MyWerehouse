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
		//public int Id { get; set; }
		public Guid Id { get; set; } = Guid.NewGuid(); //do zmiany z set na private set
		public required Guid PickingPalletId { get; set; }//paleta na której jest towar - kompletacyjna
		//public required string PickingPalletNumber { get; set; }//paleta na której jest towar - kompletacyjna
		//public string? SourcePalletId { get; set; }//paleta źródłowa na nią może wrócić towar 
		public Guid? SourcePalletId { get; set; }//paleta źródłowa na nią może wrócić towar 
		//public string? SourcePalletNumber { get; set; }//paleta źródłowa na nią może wrócić towar 
		//public string? DestinationPalletId { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
		public Guid? DestinationPalletId { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
		//public string? DestinationPalletNumber { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
		public Guid ProductId { get; set; }
		public DateOnly? BestBefore { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus Status { get; set; }
		//public int PickingTaskId { get; set; }
		public Guid PickingTaskId { get; set; }
		public PickingTask PickingTask {  get; set; }
		public DateOnly DateMade { get; set; }
		public string UserId { get; set; }
		public void AddHistory(string userId, Guid issueId, int issueNumber, ReversePickingStatus before, ReversePickingStatus after)
		{
			this.AddDomainEvent(new CreateHistoryReversePickingNotification(
				Id,
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
