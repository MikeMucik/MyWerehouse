using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class GetVirtualPalletsService : IGetVirtualPalletsService
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		public GetVirtualPalletsService(IPickingPalletRepo pickingPalletRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
		}
		public async Task<List<VirtualPallet>> GetVirtualPalletsAsync(Guid productId, DateOnly bestBefore)
		{
			return await _pickingPalletRepo.GetVirtualPalletsByBBAsync(productId, bestBefore);
		}
	}
}
