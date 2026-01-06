using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.PickingPallets.DTOs
{
	public class PickingPalletWithLocationDTO
	{
		public string PalletId { get; set;}
		public string LocationName { get; set; }
		public DateTime AddedToPicking { get; set; }		
	}
}
