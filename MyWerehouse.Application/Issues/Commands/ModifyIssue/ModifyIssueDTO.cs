using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public class ModifyIssueDTO 
	{
		public int ClientId { get; init; }
		public required string PerformedBy { get; init; }
		public List<IssueItemDTO> IssueItems { get; init; } = new List<IssueItemDTO>();		
	}
}
