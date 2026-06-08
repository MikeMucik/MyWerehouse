using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	public class PickingGuideLineDTO
	{
		public int ClientIdOut { get; set; }
		public List<IssueForPickingDTO> IssuesDetailsForPicking { get; set; } = new List<IssueForPickingDTO>();
	}
}
