using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public interface IGetVirtualPalletsService
	{
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(int productId, DateOnly bestBefore);
	}
}
