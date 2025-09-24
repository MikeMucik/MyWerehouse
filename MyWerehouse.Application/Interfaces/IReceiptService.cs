using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;

namespace MyWerehouse.Application.Interfaces
{
	public interface IReceiptService
	{		
		Task<int> CreateReceiptPlanAsync(CreateReceiptPlanDTO createReceiptPlanDTO);
		Task<string> AddPalletToReceiptAsync(int receiptId, CreatePalletReceiptDTO createReceiptPlanDTO);
		Task CompletePhysicalReceiptAsync(int receiptId, string userId);
		Task VerifyAndFinalizeReceiptAsync(int receiptId, string userId);
		Task UpdateReceiptPalletsAsync(ReceiptDTO updatingReceipt, string userId);
		Task<ReceiptDTO> GetReceiptDTOAsync(int receiptId);
		Task CancelReceiptAsync(int receiptId, string userId);
	}
}
