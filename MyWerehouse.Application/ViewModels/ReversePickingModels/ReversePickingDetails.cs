using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels.ReversePickingModels
{
	public class ReversePickingDetails
	{
		public ReversePickingDTO ReversePickingDTO { get; set; }
		public bool CanReturnToSource { get; set; }
		public bool CanAddToExistingPallet { get; set; }
		public bool AddToNewPallet { get; set; } = true;

	}
}
