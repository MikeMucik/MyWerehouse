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
		public DateOnly? DateTimeStartSend { get; set; } //issue data wysyłki,
		public DateTime? DateTimeStart { get; set; } // receipt data przyjęcia, issue data utworzenia
		public DateOnly? DateTimeEndSend { get; set; } //issue data wysyłki
		public DateTime? DateTimeEnd { get; set; } // receipt data przyjęcia, issue data utworzenia
		public string? UserId { get; set; }
		
	}
}
