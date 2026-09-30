namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletDTO
	{
		public Guid ProductId { get; init; }
		public string ProductSKU { get; init; } = string.Empty;
		public string ProductName { get; init; } = string.Empty;
		public int Quantity { get; init; }
		public DateTime DateAdded { get; init; }
		public DateOnly? BestBefore { get; init; } // May be null if the product has no best-before date
	}
}
