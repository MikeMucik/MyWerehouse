using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPickingTaskRepo
	{
		void AddPickingTask(PickingTask pickingTask);
		Task AddPickingTaskAsync(PickingTask pickingTask);
		void DeletePickingTask(PickingTask pickingTask);
		Task<List<PickingTask>> GetPickingTaskListAsync(Guid palletPickingId, DateTime pickingDate);
		//Task<PickingTask?> GetPickingTaskAsync(int pickinigTaskNumber);
		Task<PickingTask?> GetPickingTaskAsync(Guid guid);
		Task<List<PickingTask>> GetPickingTasksByIssueIdProductIdAsync(Guid issueId, Guid productId);
		Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(Guid pickingPalletId);
		Task<List<PickingTask>> GetPickingTasksByIssueIdAsync(Guid issueId);
		Task<List<PickingTask>> GetPickingTasksProductIdAsync(Guid productId, DateTime from, DateTime to);
		Task<List<VirtualPallet>> GetVirtualPalletsByIssue(Guid issueId);
		//Task<int> GetNextNumberOfPickingTask();
	}
}
