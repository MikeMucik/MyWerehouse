using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Application.Interfaces
{
	public interface IReceiptService
	{
		Task<ReceiptResult> CreateReceiptPlanAsync(CreateReceiptPlanDTO createReceiptPlanDTO);
		Task<ReceiptResult> AddPalletToReceiptAsync(int receiptId, CreatePalletReceiptDTO createReceiptPlanDTO);
		Task<ReceiptResult> CompletePhysicalReceiptAsync(int receiptId, string userId);
		Task<ReceiptResult> VerifyAndFinalizeReceiptAsync(int receiptId, string userId);
		Task<ReceiptResult> UpdateReceiptPalletsAsync(ReceiptDTO updatingReceipt, string userId);
		Task<ReceiptDTO> GetReceiptDTOAsync(int receiptId);
		Task<ReceiptResult> CancelReceiptAsync(int receiptId, string userId);
		Task<List<ReceiptDTO>> GetReceiptDTOsAsync(IssueReceiptSearchFilter filter);
	}
}
