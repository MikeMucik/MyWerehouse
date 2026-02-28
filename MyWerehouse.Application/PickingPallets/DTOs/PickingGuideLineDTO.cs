using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.PickingPallets.DTOs
{
	public class PickingGuideLineDTO
	{
		public int ClientIdOut { get; set; }
		public List<IssueForPickingDTO> IssuesDetailsForPicking { get; set; } = new List<IssueForPickingDTO>();
	}
}
