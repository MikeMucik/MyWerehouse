using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public interface IReduceAllocationService
	{
		Task ReduceAllocation(Issue issues, Guid productId,int quatnity, string userId);
	}
}
