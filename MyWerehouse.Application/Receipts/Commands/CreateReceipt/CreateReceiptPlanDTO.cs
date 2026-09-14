using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanDTO 
	{
		public int ClientId { get; init; }
		public DateTime ReceiptDateTime { get; init; }
		public required string PerformedBy { get; init; }
		public ReceiptStatus ReceiptStatus { get; init; }
		public int RampNumber { get; init; }
	}
}
