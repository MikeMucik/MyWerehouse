namespace MyWerehouse.Application.Pickings.Queries.GetListIssueToPickingTree
{
	public class PickingGuideLineDTO
	{
		public int ClientIdOut { get; init; }
		public List<IssueForPickingDTO> IssuesDetailsForPicking { get; init; } = new List<IssueForPickingDTO>();
	}
}
