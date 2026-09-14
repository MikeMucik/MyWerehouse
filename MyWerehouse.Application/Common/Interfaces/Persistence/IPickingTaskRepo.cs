using MyWerehouse.Domain.Picking.Models;
namespace MyWerehouse.Application.Common.Interfaces.Persistence
{
	public interface IPickingTaskRepo
	{
		void AddPickingTask(PickingTask pickingTask);
		Task AddPickingTaskAsync(PickingTask pickingTask, CancellationToken ct);
		void DeletePickingTask(PickingTask pickingTask);
		Task<PickingTask?> GetPickingTaskAsync(Guid guid, CancellationToken ct);
		Task<List<PickingTask>> GetPickingTasksByIssueIdProductIdAsync(Guid issueId, Guid productId, CancellationToken ct);
		Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(Guid pickingPalletId, CancellationToken ct);
		Task<List<PickingTask>> GetPickingTasksByIssueIdAsync(Guid issueId, CancellationToken ct);
		Task<List<PickingTask>> GetHandPickingTask(Guid issueId, CancellationToken ct);
	}
}
