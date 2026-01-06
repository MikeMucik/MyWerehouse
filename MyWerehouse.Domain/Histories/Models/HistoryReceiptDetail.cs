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
		public string PalletId { get; set; }
		public int LocationId { get; set; } // tu będzie lokalizacja określająca na której rampie przyjęto	
		public string? LocationSnapShot {  get; set; }
		public int HistoryReceiptId { get; set; }
		public virtual HistoryReceipt HistoryReceipt { get; set; }
	}
}
