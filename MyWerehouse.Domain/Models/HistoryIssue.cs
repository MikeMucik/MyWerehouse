using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class HistoryIssue
	{
		public int Id { get; set; }
		public int IssueId { get; set; }
		public Issue Issue { get; set; }
		public int ClientId { get; set; }
		//public Client Client { get; set; }
		public IssueStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
		public virtual ICollection<HistoryIssueDetail> Details { get; set; } = new List<HistoryIssueDetail>();
		public virtual ICollection<HistoryIssueItems> Items { get; set; } = new List<HistoryIssueItems>();
	}
}
