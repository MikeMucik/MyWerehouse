using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class HistoryReceipt
	{
		public int Id { get; set; }
		public int ReceiptId { get; set; }
		public int ClientId { get; set; } //migracja bo dodane pole
		public ReceiptStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
		public virtual ICollection<HistoryReceiptDetail> Details { get; set; } = new List<HistoryReceiptDetail>();
	}
}
