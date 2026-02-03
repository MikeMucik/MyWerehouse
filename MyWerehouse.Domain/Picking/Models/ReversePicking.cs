using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Picking.Models
{
	public class ReversePicking
	{
		public int Id { get; set; }
		public required string PickingPalletId { get; set; }//paleta na której jest towar - kompletacyjna
		public string? SourcePalletId { get; set; }//paleta źródłowa na nią może wrócić towar 
		public string? DestinationPalletId { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć lub inna o dobrych parametrach
		public int ProductId { get; set; }
		public DateOnly? BestBefore { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus Status { get; set; }
		public int PickingTaskId { get; set; }
		public PickingTask PickingTask {  get; set; }
		public DateOnly DateMade { get; set; }
		public string UserId { get; set; }
	}
}
