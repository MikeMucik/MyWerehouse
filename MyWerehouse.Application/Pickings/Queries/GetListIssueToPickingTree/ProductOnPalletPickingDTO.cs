namespace MyWerehouse.Application.Pickings.Queries.GetListIssueToPickingTree
{
	public class ProductOnPalletPickingDTO
	{
		public Guid ProductId { get; init;}
		public string SKU { get; init; } = string.Empty;
		public int Quantity {  get; init;}
	}
}
