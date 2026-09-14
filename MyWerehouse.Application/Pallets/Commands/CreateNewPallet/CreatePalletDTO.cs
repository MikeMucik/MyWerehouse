using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreatePalletDTO
	{
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }		
		public PalletStatus Status { get; init; } = 0; 
		public required string UserId { get; init; }
		public ICollection<ProductOnPalletCreateDTO> ProductsOnPallet { get; init; } = new HashSet<ProductOnPalletCreateDTO>();//tworzę paletę razem z towarem
	}
}
