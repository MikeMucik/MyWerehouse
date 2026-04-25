using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Picking.Models
{
	public enum PickingMode//w planie do użycia
	{
		Palnned = 0,
		Corrected = 1,
		Emergency = 2,
	}
}
