using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptDTO 
	{		
		public int ClientId { get; init; }
		public DateTime ReceiptDateTime { get; init; }
		public ICollection<EditPalletInReceiptDTO> Pallets { get; init; } = new List<EditPalletInReceiptDTO>();
		public required string PerformedBy { get; init; } 
		public ReceiptStatus ReceiptStatus { get; init; }
		public int RampNumber { get; init; }		
	}
}
