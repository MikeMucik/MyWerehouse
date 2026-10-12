namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryPalletDetail
	{
		public int  Id { get; set; }
		public int HistoryPalletId { get; set; }
		public virtual HistoryPallet HistoryPallet { get; set; } = null!;
		public Guid ProductId { get; set; }
		public int Quantity { get; set; } 
		public DateOnly? BestBefore { get; set; }
		public HistoryPalletDetail() { }
		public HistoryPalletDetail(Guid productId, int quantity, DateOnly? bestBefore)
		{
			ProductId = productId;
			Quantity = quantity;
			BestBefore = bestBefore;
		}	
	}

}
