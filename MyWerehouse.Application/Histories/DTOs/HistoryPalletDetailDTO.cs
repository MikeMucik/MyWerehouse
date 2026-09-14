namespace MyWerehouse.Application.Histories.DTOs
{
	public class HistoryPalletDetailDTO 
	{		
		public Guid ProductId { get; set; }		
		public int QuantityChange { get; set; } //+/-		
	}
}
