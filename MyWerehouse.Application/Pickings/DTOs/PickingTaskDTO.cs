using MyWerehouse.Domain.Pickings.Models;

namespace MyWerehouse.Application.Pickings.DTOs
{
	public class PickingTaskDTO 
	{
		public Guid Id { get; init; }
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public Guid? SourcePalletId { get; init; }      
		public string? SourcePalletNumber { get; init; }      
		public Guid ProductId { get; init; }
		public string SKU { get; init; } = string.Empty;
		public int RequestedQuantity { get; init; }
		public int PickedQuantity { get; init; }//Actual picked quantity
		public PickingStatus PickingStatus { get; init; }
		public DateOnly? BestBefore { get; init; }	
	}
}
