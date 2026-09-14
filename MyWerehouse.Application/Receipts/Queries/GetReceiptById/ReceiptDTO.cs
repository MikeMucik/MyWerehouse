using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptById
{
	public class ReceiptDTO
	{
			public Guid ReceiptId { get; init; }
			public int ReceiptNumber { get; init; }
			public int ClientId { get; init; }
			public string ClientName { get; set; } = string.Empty;
			public DateTime ReceiptDateTime { get; init; }
			public ICollection<PalletForReceiptViewDTO> Pallets { get; init; } = new List<PalletForReceiptViewDTO>();
			public string PerformedBy { get; init; } = string.Empty;
			public ReceiptStatus ReceiptStatus { get; init; }
			public int RampNumber { get; init; }		
	}
}
