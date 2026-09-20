namespace MyWerehouse.Application.Pickings.Queries.GetListIssueToPickingTree
{
	public class IssueForPickingDTO
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public List<ProductOnPalletPickingDTO> Products { get; init; } = new List<ProductOnPalletPickingDTO>();
	}
}
