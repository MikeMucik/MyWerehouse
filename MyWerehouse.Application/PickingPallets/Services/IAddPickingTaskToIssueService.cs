using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public interface IAddPickingTaskToIssueService
	{
		Task<AddPickingTaskToIssueResult> AddPickingTaskToIssue(List<Pallet> pallets,
			List<VirtualPallet> virtualPallets, Issue issue, Guid productId,
			int rest, DateOnly? bestBefore, string userId);
		//Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(
		//	VirtualPallet virtualPallet, Issue issue, int productId,
		//	int quantity, DateOnly? bestBefore, string userId);

		AddPickingTaskToIssueResult AddOnePickingTaskToIssue(
			VirtualPallet virtualPallet, Issue issue, Guid productId,
			int quantity, DateOnly? bestBefore, string userId);
	}
}
