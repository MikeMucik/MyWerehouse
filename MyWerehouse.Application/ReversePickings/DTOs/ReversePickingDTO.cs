using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.ReversePickings.DTOs
{
	public class ReversePickingDTO
	{
		public Guid Id { get; init; }
		public required string PickingPalletNumber { get; init; }
		public string? SourcePalletNumber { get; init; }
		public required string ProductSKU { get; init; }		
		public DateOnly? BestBefore { get; init; }
		public int Quantity { get; init; }
		public ReversePickingStatus Status { get; init; }		
	}
}
