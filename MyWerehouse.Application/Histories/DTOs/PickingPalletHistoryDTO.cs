using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class PickingPalletHistoryDTO
	{
		public int Id { get; set; }
		public int? PickingTaskId { get; set; }									  //[JsonIgnore] // Ignoruj przy serializacji
		public string PalletId { get; set; } = string.Empty;
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int QuantityAllocated { get; set; }   
		public int QuantityPicked { get; set; }      
		public PickingStatus StatusBefore { get; set; }
		public PickingStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; } = string.Empty;
		public DateTime DateTime { get; set; }		
	}
}
