using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListPickingPalletForOperator
{
	public class PickingPalletWithLocationDTO
	{
		public Guid PalletId { get; init;}
		public string PalletNumber { get; init;}
		public int LocationId { get; init;}
		public string LocationName { get; init; }
		public DateTime AddedToPicking { get; init; }		
	}
}
