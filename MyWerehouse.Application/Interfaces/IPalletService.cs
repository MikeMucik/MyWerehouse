using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPalletService
	{				
		Task<PalletResult> CreatePalletAsync(PalletDTO addPalletDTO, string userId);		//dodanie palety niepowiązanej
		Task<PalletResult> DeletePalletAsync(string id, string UserId);	
		Task<UpdatePalletDTO> GetPalletToEditAsync(string id);		
		Task<PalletResult> UpdatePalletAsync(UpdatePalletDTO updatingPallet, string userId);		
		//Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id);		
		Task<ChangeLocationResults> ChangeLocationPalletAsync(string palletId, int destinationLocation, string userId, bool force = false);
		Task <List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter);
		//Task<VirtualPallet> AddPalletToPickingAsync(Issue issue, int productId, DateOnly? bestBefore, string userId);
		//Task<List<Pallet>> GetAllAvailablePalletsAsync(int productId, DateOnly? bestBefore);
		//TODO: przestawienie 
	}
}
