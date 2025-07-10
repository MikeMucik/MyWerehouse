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
		//void AddReceipt(AddReceiptDTO addReceiptDTO);
		//int CreateReceiptPlan(CreateReceiptPlanDTO createReceiptPlanDTO);
		Task<int> CreateReceiptPlanAsync(CreateReceiptPlanDTO createReceiptPlanDTO);
		//string AddPalletToReceipt(int receiptId, CreatePalletReceiptDTO createReceiptPlanDTO);
		Task<string> AddPalletToReceiptAsync(int receiptId, CreatePalletReceiptDTO createReceiptPlanDTO);
		//void CompletePhysicalReceipt(int receiptId, string userId);
		Task CompletePhysicalReceiptAsync(int receiptId, string userId);
		//void VerifyAndFinalizeReceipt(int receiptId, string userId);
		Task VerifyAndFinalizeReceiptAsync(int receiptId, string userId);
		//void UpdateReceiptPallets(ReceiptDTO updatingReceipt, string userId);
		Task UpdateReceiptPalletsAsync(ReceiptDTO updatingReceipt, string userId);
		//ReceiptDTO GetReceiptDTO(int receiptId);
		Task<ReceiptDTO> GetReceiptDTOAsync(int receiptId);
		//void DeleteReceipt(int receipt, string userId);
	}
}
