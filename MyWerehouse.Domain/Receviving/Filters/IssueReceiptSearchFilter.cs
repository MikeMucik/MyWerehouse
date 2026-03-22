using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Receviving.Filters
{
	public class IssueReceiptSearchFilter
	{
		public Guid IssueId { get; set; }
		public Guid ReceiptId { get; set; }
		public int? ClientId { get; set; }
		public string? ClientName { get; set; }
		public Guid? ProductId { get; set; }//
		public string? ProductName { get; set; }
		
		//public int Iss
		public DateTime? DateTimeStart { get; set; } //zakres dat dla issue data wysyłki, receipt dzień przyjęcia
		public DateTime? DateTimeEnd { get; set; } //zakres dat dla issue data wysyłki, receipt dzień przyjęcia
		public string? UserId { get; set; }


		public string? SKU { get; set; } //TODO
	}
}
