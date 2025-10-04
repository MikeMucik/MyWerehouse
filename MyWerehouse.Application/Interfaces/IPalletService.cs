using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPalletService
	{				
		//Task<string> CreatePickingPalletAsync(CreatePalletPickingDTO addPalletDTO);		
		Task DeletePalletAsync(string id);	
		Task<UpdatePalletDTO> GetPalletToEditAsync(string id);		
		Task UpdatePalletAsync(UpdatePalletDTO updatingPallet);		
		Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id);		
		Task<ChangeLocationResults> ChangeLocationPalletAsync(string palletId, int destinationLocation, string userId, bool force = false);
		Task <List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter);
		Task<VirtualPallet> AddPalletToPickingAsync(Issue issue, int productId, DateOnly? bestBefore, string userId);
		Task<List<Pallet>> GetAllAvailablePalletsAsync(int productId, DateOnly? bestBefore);
	}
}
