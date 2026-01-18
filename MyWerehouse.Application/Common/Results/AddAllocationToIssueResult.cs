using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Common.Results
{
	public class AddAllocationToIssueResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		//public List<Allocation> Allocations { get; set; }
		public AddAllocationToIssueResult()	{	}
		public static AddAllocationToIssueResult Ok()
		{
			return new AddAllocationToIssueResult
			{
				Success = true,				
			};
		}
		public static AddAllocationToIssueResult Fail(string message)
		{
			return new AddAllocationToIssueResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
