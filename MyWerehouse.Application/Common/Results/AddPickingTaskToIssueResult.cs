using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Common.Results
{
	public class AddPickingTaskToIssueResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public List<PickingTask> PickingTask { get; set; }
		public PickingTask OnePickingTask { get; set; }
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
				OnePickingTask = onePickingTask
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
