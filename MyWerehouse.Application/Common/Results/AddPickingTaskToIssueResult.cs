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
		public AddPickingTaskToIssueResult()	{	}
		public static AddPickingTaskToIssueResult Ok(List<PickingTask> PickingTask)
		{
			return new AddPickingTaskToIssueResult
			{
				Success = true,	
				PickingTask = PickingTask
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
