using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	public class IssueForPickingDTO
	{
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public List<ProductOnPalletPickingDTO> Products { get; set; } = new List<ProductOnPalletPickingDTO>();
	}
}
