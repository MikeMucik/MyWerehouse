using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryReceiptDetail
	{
		public int Id { get; set; }
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; } = string.Empty;
		public int LocationId { get; set; } // tu będzie lokalizacja określająca na której rampie przyjęto	
		public string? LocationSnapShot {  get; set; }
		public int HistoryReceiptId { get; set; }
		public HistoryReceipt HistoryReceipt { get; set; } = null!;
	}
}
