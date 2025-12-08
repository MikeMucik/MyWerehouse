using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Interfaces
{
	public interface IReversePickingService
	{
		Task<List<ReversePickingResult>> CreateTaskToReversePickingAsync(string palletId);
	}
}
