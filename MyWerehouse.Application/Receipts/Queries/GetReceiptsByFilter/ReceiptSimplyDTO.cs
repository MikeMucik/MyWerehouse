using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter
{
	public class ReceiptSimplyDTO
	{
		public Guid ReceiptId { get; set; }
		public int ReceiptNumber { get; set; }
		public int ClientId { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public string PerformedBy { get; set; } = string.Empty; 
		public ReceiptStatus ReceiptStatus { get; set; }
		public int RampNumber { get; set; }		
	}
}
