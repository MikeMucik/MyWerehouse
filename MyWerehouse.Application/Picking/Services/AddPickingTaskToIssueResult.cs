using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.Services
{
	public sealed class AddPickingTaskToIssueResult
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public List<PickingTask> PickingTask { get; set; } = [];
		public AddPickingTaskToIssueResult()	{	}
		public static AddPickingTaskToIssueResult Ok(List<PickingTask> pickingTask)
		{
			return new AddPickingTaskToIssueResult
			{
				Success = true,	
				PickingTask = pickingTask
			};
		}
		public static AddPickingTaskToIssueResult Ok(PickingTask onePickingTask)
		{
			return new AddPickingTaskToIssueResult
			{
				Success = true,
				PickingTask = [onePickingTask]
			};
		}
		public static AddPickingTaskToIssueResult Fail(string message)
		{
			return new AddPickingTaskToIssueResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
