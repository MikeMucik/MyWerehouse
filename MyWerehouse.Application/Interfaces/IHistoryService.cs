using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.HistoryDTO;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IHistoryService
	{
		//Task CreateHistoryAllocationAsync(VirtualPallet virtualPallet, PickingStatus status, string userId, IEnumerable<HistoryPickingDetail> details);
		void CreateHistoryPicking(VirtualPallet virtualPallet, Allocation allocation, string performedBy, PickingStatus statusBefore, int quantityPicked);
		void CreateHistoryPicking(VirtualPallet virtualPallet, Allocation allocation, string performedBy, PickingStatus statusBefore);
		//Task CreateHistoryIssueAsync(Issue issue, string userId, IEnumerable<HistoryIssueDetail> details);
		void CreateHistoryIssue(Issue issue);
		//Task CreateHistoryIssueAsync(Issue issue);
		void CreateHistoryReceipt(Receipt receipt);	
		void CreateHistoryReceipt(Receipt receipt, ReceiptStatus receiptStatus, string userId);	
		void CreateMovement(Pallet pallet, Location destinationLocation , ReasonMovement reasonMovement, string userId, 
			PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details);
		void CreateOperation(Pallet pallet, string userId, PalletStatus newStatus); //overload
		void CreateOperation(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovementstring, string userId,
			PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details);
		Task <PalletHistoryDTO> GetHistoryPalletByIdAsync(string  id);
		Task <ReceiptHistoryDTO> GetHistoryReceiptByIdAsync(string  id);
		Task <IssueHistoryDTO> GetHistoryIssueByIdAsync(string  id);
	}
}
