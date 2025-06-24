using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.PalletModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPalletService
	{
		string AddPalletReceipt(CreatePalletReceiptDTO addPalletDTO);
		Task <string> AddPalletReceiptAsync(CreatePalletReceiptDTO addPalletDTO);
		string CreatePickingPallet(CreatePalletPickingDTO addPalletDTO);
		Task <string> CreatePickingPalletAsync(CreatePalletPickingDTO addPalletDTO);
		void DeletePallet(string id);
		Task DeletePalletAsync(string id);
		//CreatePalletReceiptDTO GetPalletToEdit(string id);
		UpdatePalletDTO GetPalletToEdit(string id);
		void UpdatePallet(UpdatePalletDTO updatingPallet);
	}
}
