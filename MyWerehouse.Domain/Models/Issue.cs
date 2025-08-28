using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class Issue
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime? IssueDateTimeSend { get; set; } 
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public virtual ICollection<HistoryIssue> HistoryIssues { get; set; } = new List<HistoryIssue>();
		public string? PerformedBy { get; set; } // opcjonalnie: user
		public IssueStatus IssueStatus { get; set; } // migracja
		public string? SendedBy { get; set; } //migracja												
	}
}
