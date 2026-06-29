using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Receviving.Filters
{
	public class IssueReceiptSearchFilter
	{
		public int? IssueNumber { get; set; }
		public int? ReceiptNumber { get; set; }
		public int? ClientId { get; set; }
		public string? ClientName { get; set; }
		public Guid? ProductId { get; set; }//
		public string? SKU { get; set; } 
		public string? ProductName { get; set; }		
		public DateOnly? SendDateStart { get; set; } //issue data wysyłki,
		public DateTime? CreateDateStart { get; set; } // receipt data przyjęcia, issue data utworzenia
		public DateOnly? SendDateEnd { get; set; } //issue data wysyłki
		public DateTime? CreateDateEnd { get; set; } // receipt data przyjęcia, issue data utworzenia
		public string? UserId { get; set; }
		
	}
}
