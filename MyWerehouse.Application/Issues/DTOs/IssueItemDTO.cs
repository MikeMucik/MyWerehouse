using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public record IssueItemDTO 
	{	
		public Guid ProductId { get; init; }
		public int Quantity { get; init; }
		public DateOnly? BestBefore { get; init; }			
	}
}
