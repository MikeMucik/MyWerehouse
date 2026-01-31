using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPickingPalletService
	{
		Task<List<PickingPalletWithLocationDTO>> GetListPickingPalletAsync(DateOnly dateMovedStart, DateOnly dateMovedEnd);
		Task<List<ProductToIssueDTO>> GetListToPickingAsync(DateOnly dateIssueStart, DateOnly dateIssueEnd); //podaje pojedyncze alokacje
		Task<List<PickingGuideLineDTO>> GetListIssueToPickingAsync(DateOnly dateIssueStart, DateOnly dateIssueEnd); //podaje według klienta -> drzewko		
		Task <List<PickingTaskDTO>> ShowTaskToDoAsync(string palletSouceScanned, DateTime pickingDate);
		Task<PickingResult> DoPickingAsync(PickingTaskDTO pickingTaskDTO, string userId);		
		Task<PrepareCorrectedPickingResult> PrepareManualPickingAsync(string palletId);
		Task<PickingResult> ExecuteManualPickingAsync(string palletId, int issueId, string userId);
		Task<PickingResult> ClosePickingPalletAsync(string palletId, int issueId, string userId);
	}
}
