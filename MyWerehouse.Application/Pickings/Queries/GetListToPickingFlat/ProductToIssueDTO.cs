namespace MyWerehouse.Application.Pickings.Queries.GetListToPickingFlat
{
	public class ProductToIssueDTO
	{
		public int ClientIdOut { get; init; }
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public Guid ProductId { get; init; }
		public string SKU { get; init; } = string.Empty;
		public int Quantity { get; init; }		
	}
}
