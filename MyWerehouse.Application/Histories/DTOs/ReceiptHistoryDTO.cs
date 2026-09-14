using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public	class ReceiptHistoryDTO
	{
		public int Id { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public int CLientId { get; set; }
		public ReceiptStatus ReceiptStatus { get; set; }
		public string PerformedBy { get; set; } = string.Empty;
		public ICollection<PalletListDTO> ListDTOs { get; set; } = new List<PalletListDTO>();
	}
}
