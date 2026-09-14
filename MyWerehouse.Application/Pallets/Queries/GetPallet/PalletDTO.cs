using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetPallet
{
	public class PalletDTO 
	{
		public string PalletNumber { get; init; } = string.Empty;
		public DateTime DateReceived { get; init; }
		public string LocationSnapShot { get; init; } = string.Empty;
		public PalletStatus Status { get; init; } = 0; 
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new HashSet<ProductOnPalletDTO>();
		public ICollection<HistoryPalletDTO> PalletHistory { get; init; } = new List<HistoryPalletDTO>();
		public int? ReceiptNumber { get; init; }     
		public int? IssueNumber { get; init; }		
	}
}
