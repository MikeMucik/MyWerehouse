using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class HistoryIssueDetail
	{
		public int Id { get; set; }
		public string PalletId { get; set; }
		public int LocationId { get; set; }		
		public int HistoryIssueId { get; set; }
		public virtual HistoryIssue HistoryIssue { get; set; }
	}
}
