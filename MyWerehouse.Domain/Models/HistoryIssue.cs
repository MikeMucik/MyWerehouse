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
		public IssueStatus Status { get; set; }
		public string? PerfomedBy { get; set; }
		public DateTime DateTime { get; set; }
		public virtual ICollection<HistoryIssueDetail> Details { get; set; } = new List<HistoryIssueDetail>();
	}
}
