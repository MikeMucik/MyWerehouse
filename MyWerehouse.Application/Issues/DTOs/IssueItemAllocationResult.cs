using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueItemAllocationResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public Guid ProductId { get; set; }		
		public int QuantityRequest { get; set; }
		public int QuantityOnStock { get; set; }	

		public IssueItemAllocationResult() { }

		public static IssueItemAllocationResult Ok(
			string message,
			Guid product)
		{
			return new IssueItemAllocationResult
			{
				Success = true,
				Message = message,
				ProductId = product
			};
		}

		public static IssueItemAllocationResult Ok(
			string message)
		{
			return new IssueItemAllocationResult
			{
				Success = true,
				Message = message,
			};
		}
		public static IssueItemAllocationResult Fail(
			string message,
			Guid productNotAdded,
			int issueQuantity,
			int onStock)
		{
			return new IssueItemAllocationResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded,
				QuantityRequest = issueQuantity,
				QuantityOnStock = onStock
			};
		}
		public static IssueItemAllocationResult Fail(
			string message,
			Guid productNotAdded)
		{
			return new IssueItemAllocationResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded,
			};
		}
		public static IssueItemAllocationResult Fail(string message)
		{
			return new IssueItemAllocationResult
			{
				Success = false,
				Message = message				
			};
		}		
	}
}
