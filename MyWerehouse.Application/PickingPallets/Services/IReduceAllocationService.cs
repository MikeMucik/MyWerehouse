using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public interface IReduceAllocationService
	{
		Task ReduceAllocation(Issue issues, int productId, string userId);
	}
}
