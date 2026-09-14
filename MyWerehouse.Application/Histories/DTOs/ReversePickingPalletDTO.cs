using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class ReversePickingPalletDTO
	{
		public int Id { get; set; }
		public int ReversePickingId { get; set; }
		public string? PalletSourceId { get; set; }
		public string? PalletDestinationId { get; set; }
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus? StatusBefore { get; set; }
		public ReversePickingStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; } = string.Empty;
		public DateTime DateTime { get; set; }		
	}
}
