using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking
{
	public class IssueOptions
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public int QunatityToDo { get; init; }
	}
}
