using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Results
{
	public class ProcessPickingActionResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public ProcessPickingActionResult() { }
		public static ProcessPickingActionResult Ok()
		{
			return new ProcessPickingActionResult
			{
				Success = true
			};
		}
		public static ProcessPickingActionResult Fail(string message)
		{
			return new ProcessPickingActionResult
			{
				Success = false,
				Message = message
			};
		}
	}
}