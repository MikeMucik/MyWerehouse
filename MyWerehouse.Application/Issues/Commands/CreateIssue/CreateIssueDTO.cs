using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public class CreateIssueDTO
	{		
		public int ClientId { get; init; }
		public required string PerformedBy { get; init; } 
		public List<IssueItemDTO> Items { get; init; } = new List<IssueItemDTO>();		
	}
}
