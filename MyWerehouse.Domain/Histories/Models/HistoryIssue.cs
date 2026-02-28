using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryIssue
	{
		public int Id { get; set; }
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public IssueStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
		public virtual ICollection<HistoryIssueDetail> Details { get; set; } = new List<HistoryIssueDetail>();
		public virtual ICollection<HistoryIssueItems> Items { get; set; } = new List<HistoryIssueItems>();
	}
}
