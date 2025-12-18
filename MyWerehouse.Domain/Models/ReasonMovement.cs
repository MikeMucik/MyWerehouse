using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public enum ReasonMovement
	{
		New = 0,
		Received = 1,
		Picking = 2,
		Moved = 3,
		Correction = 4,
		Merge = 5,
		ToLoad = 6,
		Loaded = 7,
		CancelIssue = 8,
		ReversePicking = 9,
		//InStock = 8,
		//Archived= 9,
	}
}
