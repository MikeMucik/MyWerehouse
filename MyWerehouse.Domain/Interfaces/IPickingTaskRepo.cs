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
		Task<List<PickingTask>> GetPickingTaskListAsync(int palletPickingId, DateTime pickingDate);
		Task<PickingTask> GetPickingTaskAsync(int pickingTaskId);
		Task<List<PickingTask>> GetPickingTasksByIssueIdProductIdAsync(int issueId, int productId);
		Task<List<PickingTask>> GetPickingTasksByPickingPalletIdAsync(string pickingPalletId);
		Task<List<PickingTask>> GetPickingTasksByIssueIdAsync(int issueId);
		Task<List<PickingTask>> GetPickingTasksProductIdAsync(int productId, DateTime from, DateTime to);
		Task<List<VirtualPallet>> GetVirtualPalletsByIssue(int issueId);		
	}
}
