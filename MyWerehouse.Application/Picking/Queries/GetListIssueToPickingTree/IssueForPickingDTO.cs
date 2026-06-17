using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	public class IssueForPickingDTO
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public List<ProductOnPalletPickingDTO> Products { get; init; } = new List<ProductOnPalletPickingDTO>();
	}
}
