using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryIssueItems
	{
		public int Id { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }
		public int HistoryIssueId { get; set; }
		public virtual HistoryIssue HistoryIssue { get; set; }
	}
}
