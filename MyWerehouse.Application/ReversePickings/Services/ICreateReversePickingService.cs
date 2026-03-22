using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public interface ICreateReversePickingService
	{
		Task<ReversePickingResult> CreateReversePicking(Guid palletId, string userId);
	}
}
