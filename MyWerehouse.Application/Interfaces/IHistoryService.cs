using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IHistoryService
	{
		//Task CreateHistoryAllocationAsync(VirtualPallet virtualPallet, PickingStatus status, string userId, IEnumerable<HistoryPickingDetail> details);
		Task CreateHistoryPickingAsync(VirtualPallet virtualPallet, Allocation allocation, string performedBy, PickingStatus statusBefore, int quantityPicked);
		Task CreateHistoryPickingAsync(VirtualPallet virtualPallet, Allocation allocation, string performedBy, PickingStatus statusBefore);
		//Task CreateHistoryIssueAsync(Issue issue, string userId, IEnumerable<HistoryIssueDetail> details);
		Task CreateHistoryIssueAsync(Issue issue);
		//Task CreateHistoryIssueAsync(Issue issue);
		Task CreateHistoryReceiptAsync(Receipt receipt);	
		Task CreateHistoryReceiptAsync(Receipt receipt, ReceiptStatus receiptStatus, string userId);	
		Task CreateMovementAsync(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, 
			PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details);
		Task CreateMovementAsync(Pallet pallet, string userId, PalletStatus newStatus); //overload
	}
}
