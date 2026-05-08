using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Picking.Models
{
	public class PickingTaskFlat
	{
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
	}
}
