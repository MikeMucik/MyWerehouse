using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking
{ 
	public class ReversePickingPalletWithLocationDTO 
	{
		public Guid PalletId { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public string LocationName { get; init; } = string.Empty;
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; }
	}
}
