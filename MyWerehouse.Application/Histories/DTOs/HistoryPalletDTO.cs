using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class HistoryPalletDTO
	{
		public int Id { get; set; }
		public required string PalletNumber { get; set; }		
		public string? LocationSnapShotSource { get; set; } = string.Empty;
		public string? LocationSnapShotDestination { get; set; } = string.Empty;
		public ReasonForPallet Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string PerformedBy { get; set; } = string.Empty; // opcjonalnie: user		
		public ICollection<HistoryPalletDetailDTO> HistoryPalletDetailsDTO { get; set; } = new List<HistoryPalletDetailDTO>();
		public DateTime MovementDate { get; set; }
	}
}
