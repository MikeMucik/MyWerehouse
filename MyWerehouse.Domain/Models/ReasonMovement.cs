using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public enum ReasonMovement
	{
		Unknown = 0,
		Received = 1,
		Picking = 2,
		Correction = 3,
		Merge = 4,
		Split = 5,
		ManualMove = 6,
		Loaded = 7,
	}
}
