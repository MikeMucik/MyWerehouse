using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.DTOs;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public interface ICreateReversePickingService
	{
		Task<ReversePickingResult> CreateReversePicking(Guid palletId, string userId);
	}
}
