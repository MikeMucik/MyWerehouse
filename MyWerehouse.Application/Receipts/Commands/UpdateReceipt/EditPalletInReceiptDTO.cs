using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class EditPalletInReceiptDTO 
	{
		public Guid Id { get; init; }
		public string? PalletNumber { get; init; }// An empty PalletNumber means a new pallet will be created while editing the receipt.
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; } = 0;
		public ICollection<ProductOnPalletUpdateDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletUpdateDTO>();
		public Guid? ReceiptId { get; init; }
	}
}
