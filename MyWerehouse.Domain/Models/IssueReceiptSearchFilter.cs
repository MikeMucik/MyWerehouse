using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class IssueReceiptSearchFilter
	{
		public int? ClientId { get; set; }
		public string? ClientName { get; set; }
		public int? ProductId { get; set; }
		public string? ProductName { get; set; }
		public DateTime? DateTimeStart { get; set; } //zakres dat
		public DateTime? DateTimeEnd { get; set; } //zakres dat
		public string? UserId { get; set; }
	}
}
