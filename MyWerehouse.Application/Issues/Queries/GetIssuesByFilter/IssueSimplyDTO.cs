using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFilter
{
	public class IssueSimplyDTO 
	{
		public Guid Id { get; init; }
		public int IssueNumber { get; init; }
		public int ClientId { get; init; }
		public DateTime IssueDateTimeCreate { get; init; }
		public DateOnly IssueDateTimeSend { get; init; }
		public string PerformedBy { get; init; } = string.Empty;
		public IssueStatus IssueStatus { get; init; }		
	}
}
