using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Common.Results
{
	public class IssueResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int ProductId { get; set; }
		public int QuantityRequest { get; set; }
		public int QuantityOnStock { get; set; }		
		//public IssueStatus Status { get; set; }

		public IssueResult() { }

		public static IssueResult Ok(
			string message,
			int product)
		{
			return new IssueResult
			{
				Success = true,
				Message = message,
				ProductId = product
			};
		}
		public static IssueResult Ok(
			string message)
		{
			return new IssueResult
			{
				Success = true,
				Message = message,				
			};
		}
		public static IssueResult Fail(
			string message,
			int productNotAdded,
			int issueQuantity,
			int onStock)
		{
			return new IssueResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded,
				QuantityRequest = issueQuantity,
				QuantityOnStock = onStock
			};
		}
		public static IssueResult Fail(
			string message,
			int productNotAdded)
		{
			return new IssueResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded,
			};
		}
		public static IssueResult Fail(string message)
		{
			return new IssueResult
			{
				Success = false,
				Message = message				
			};
		}
	}
}
