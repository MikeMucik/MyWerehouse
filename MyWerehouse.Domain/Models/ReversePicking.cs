using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class ReversePicking
	{
		public int Id { get; set; }
		public required string PickingPalletId { get; set; }
		public string? SourcePalletId { get; set; }//paleta źródłowa na nią wraca towar lub do której dodajemy
		public string? DestinationPalletId { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć
		public int ProductId { get; set; }
		public DateOnly? BestBefore { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus Status { get; set; }
		public int AllocationId { get; set; }
		public Allocation Allocation {  get; set; }
	}
}
