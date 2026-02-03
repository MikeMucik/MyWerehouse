using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public interface ICreateReversePickingService
	{
		Task CreateReversePicking(string palletId, string userId);
	}
}
