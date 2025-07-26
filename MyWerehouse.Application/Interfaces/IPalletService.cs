using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPalletService
	{				
		Task<string> CreatePickingPalletAsync(CreatePalletPickingDTO addPalletDTO);		
		Task DeletePalletAsync(string id);	
		Task<UpdatePalletDTO> GetPalletToEditAsync(string id);		
		Task UpdatePalletAsync(UpdatePalletDTO updatingPallet);		
		Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id);		
		Task ChangeLocationPalletAsync(string palletId, int destinationLocation, string userId);
		Task <List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter);
	}
}
