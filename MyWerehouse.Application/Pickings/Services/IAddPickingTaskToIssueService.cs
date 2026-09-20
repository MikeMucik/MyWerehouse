using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pickings.Models;

namespace MyWerehouse.Application.Pickings.Services
{
	public interface IAddPickingTaskToIssueService
	{
		Task<AddPickingTaskToIssueResult> AddPickingTasksToIssue(List<Pallet>? pallets,
			List<VirtualPallet>? virtualPallets, Issue issue, Guid productId,
			int quantity, DateOnly? bestBefore, string userId, CancellationToken ct);

		Task< AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(
			VirtualPallet virtualPallet, Issue issue, Guid productId,
			int quantity, DateOnly? bestBefore, string userId, CancellationToken ct);
	}
}
