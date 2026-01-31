using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPickingPalletRepo
	{
		VirtualPallet AddPalletToPicking(VirtualPallet virtualPallet);
		Task<VirtualPallet> AddPalletToPickingAsync(VirtualPallet virtualPallet);
		void DeleteVirtualPalletPicking(VirtualPallet virtualPallet);
		Task<List<VirtualPallet>> GetVirtualPalletsAsync(int productId);
		Task<List<VirtualPallet>> GetVirtualPalletsByTimeAsync(DateTime start, DateTime end);//chyba do wywalenia
		Task<List<VirtualPallet>> GetVirtualPalletsByTimePickingTaskAsync(DateOnly start, DateOnly end);
		Task<List<VirtualPallet>> GetVirtualPalletsByBBAsync(int productId, DateOnly bestBefore);
		//Task<DateTime> TakeDateAddedToPickingAsync(int pickingPalletId);
		Task<int> GetVirtualPalletIdFromPalletIdAsync(string palletId);
		Task<VirtualPallet> GetVirtualPalletByIdAsync(int palletId);
		void ClosePickingPallet(string palletId, int issueId);		
	}
}
