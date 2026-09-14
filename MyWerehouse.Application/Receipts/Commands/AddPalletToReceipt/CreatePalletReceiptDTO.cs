using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class CreatePalletReceiptDTO
	{
		public ICollection<ProductOnPalletCreateDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletCreateDTO>();
		public required string UserId { get; init; }
	}
}
