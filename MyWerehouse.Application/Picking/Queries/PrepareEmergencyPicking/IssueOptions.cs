using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.PrepareEmergencyPicking
{
	public class IssueOptions
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public int QuantityToDo { get; init; }
	}
}
