namespace MyWerehouse.Application.Pickings.Queries.PrepareEmergencyPicking
{
	public class IssueOptions
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public int QuantityToDo { get; init; }
	}
}
