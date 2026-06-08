using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking
{
	public class IssueOptions
	{
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public int QunatityToDo { get; set; }
	}
}
