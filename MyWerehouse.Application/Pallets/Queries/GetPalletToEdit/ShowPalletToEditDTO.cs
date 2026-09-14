using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public class ShowPalletToEditDTO 
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }
		public string? LocationSnapShot { get; init; }//
		public PalletStatus Status { get; init; } = 0;
		public ICollection<ProductOnPalletToEditDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletToEditDTO>();
	}
}
