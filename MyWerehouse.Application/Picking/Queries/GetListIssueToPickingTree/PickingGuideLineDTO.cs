using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	public class PickingGuideLineDTO
	{
		public int ClientIdOut { get; init; }
		public List<IssueForPickingDTO> IssuesDetailsForPicking { get; init; } = new List<IssueForPickingDTO>();
	}
}
