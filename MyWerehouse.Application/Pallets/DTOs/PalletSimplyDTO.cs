using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class PalletSimplyDTO
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public PalletStatus Status { get; init; }		
	}
}
