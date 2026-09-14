using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptById
{
	public class PalletForReceiptViewDTO 
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; } = 0;
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletDTO>();
	}
}
