using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.IssueModels;

namespace MyWerehouse.Application.ViewModels.PickingPalletModels
{
	public class PickingGuideLineDTO
	{
		public int ClientIdOut { get; set; }
		public List<IssueForPickingDTO> Issues { get; set; } = new List<IssueForPickingDTO>();
	}
}
