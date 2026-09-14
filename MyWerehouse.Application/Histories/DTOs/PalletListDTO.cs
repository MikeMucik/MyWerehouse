
namespace MyWerehouse.Application.Histories.DTOs
{
	public class PalletListDTO 								
	{
		public string PalletId { get; set; } = string.Empty;
		public int LocationId { get; set; }
		public string LocationSnapShot { get; set; } = string.Empty;		
	}
}
