using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.DTOs
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
		public int PickedQuantity { get; init; }//faktyczna pobrana ilość
		public PickingStatus PickingStatus { get; init; }
		public DateOnly? BestBefore { get; init; }	
	}
}
