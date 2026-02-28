using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class HandPickingRepo : IHandPickingTaskRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public HandPickingRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddHandPickingTask(HandPickingTask handPickingTask)
		{
			_werehouseDbContext.HandPickingTasks.Add(handPickingTask);
		}

		public async Task<HandPickingTask?> GetByIssueAndProductAsync(Guid issueId, int productId)
		{
			var task = await _werehouseDbContext.HandPickingTasks.FirstOrDefaultAsync(h => h.IssueId == issueId && h.ProductId == productId);
			return task;
		}

		public async Task<List<HandPickingTask>> GetHandPickingTasksAsync(DateTime startDate, DateTime endDate)
		{
			var listTasks = await _werehouseDbContext.HandPickingTasks
				.Where(x=>x.CreateDate >= startDate && x.CreateDate <= endDate)
				.ToListAsync();
			return listTasks;
		}

		//public void UpdateHandPickingTask(HandPickingTask handPickingTask)
		//{
		//	throw new NotImplementedException();
		//}
	}
}
