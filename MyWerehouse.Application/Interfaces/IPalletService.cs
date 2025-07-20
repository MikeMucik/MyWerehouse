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
		string CreatePickingPallet(CreatePalletPickingDTO addPalletDTO);
		Task<string> CreatePickingPalletAsync(CreatePalletPickingDTO addPalletDTO);
		void DeletePallet(string id);
		Task DeletePalletAsync(string id);
		UpdatePalletDTO GetPalletToEdit(string id);
		Task<UpdatePalletDTO> GetPalletToEditAsync(string id);
		void UpdatePallet(UpdatePalletDTO updatingPallet);
		Task UpdatePalletAsync(UpdatePalletDTO updatingPallet);
		PalletHistoryDTO ShowHistoryPallet(string id);
		Task<PalletHistoryDTO> ShowHistoryPalletAsync(string id);
		void ChangeLocationPallet(string palletId, int destinationLocation, string userId);
		Task ChangeLocationPalletAsync(string palletId, int destinationLocation, string userId);
		Task <List<PalletDTO>> FindPalletsByFiltrAsync(PalletSearchFilter filter);
	}
}
