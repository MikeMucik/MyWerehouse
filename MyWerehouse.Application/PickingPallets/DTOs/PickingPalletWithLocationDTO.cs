using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.PickingPallets.DTOs
{
	public class PickingPalletWithLocationDTO
	{
		public Guid PalletId { get; set;}
		public string PalletNumber { get; set;}
		public string LocationName { get; set; }
		public DateTime AddedToPicking { get; set; }		
	}
}
