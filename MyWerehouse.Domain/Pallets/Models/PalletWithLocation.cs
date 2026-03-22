using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class PalletWithLocation
	{
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; }
		public int LocationId { get; set; }
	}
}
