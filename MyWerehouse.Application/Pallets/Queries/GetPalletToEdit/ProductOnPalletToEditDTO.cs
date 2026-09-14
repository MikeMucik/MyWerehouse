namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public class ProductOnPalletToEditDTO 
	{
		public Guid ProductId { get; init; }
		public string ProductName { get; init; } = string.Empty;
		public string SKU { get; init; } = string.Empty;
		public DateTime DateAdded { get; init; }
		public int Quantity { get; init; }
		public DateOnly? BestBefore { get; init; }		
	}
}
