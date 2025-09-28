using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.ViewModels.PickingPalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPickingPalletService
	{
		Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateTime dateMovedStart, DateTime dateMovedEnd);
		Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd); //podaje pojedyncze alokacje
		Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateTime dateIssueStart, DateTime dateIssueEnd); //podaje według klienta -> drzewko		
		Task <List<AllocationDTO>> ShowTaskToDoAsync(string palletSouceScanned, DateTime pickingDate);
		Task<PickingResult> DoPickingAsync(AllocationDTO allocationDTO, string userId);		
		Task<PickingResult> PrepareManualPickingAsync(string palletId);
		Task<PickingResult> ExecuteManualPickingAsync(string palletId, int issueId, string userId);
	}
}
