using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IHandPickingTaskRepo
	{
		void AddHandPickingTask(HandPickingTask handPickingTask);		
		Task<HandPickingTask?> GetByIssueAndProductAsync(Guid issueId, int productId);
		Task<List<HandPickingTask>> GetHandPickingTasksAsync(DateTime startDate,  DateTime endDate);
		//void UpdateHandPickingTask(HandPickingTask handPickingTask);
	}
}
