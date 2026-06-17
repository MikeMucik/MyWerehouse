using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public sealed class AssignProductToIssueResult
	{
		public bool Success { get; init; }
		public string Message { get; set; }
		public Guid ProductId { get; init; }		
		public IReadOnlyList<Pallet> AssignedPallets { get; init; }
		public int QuantityRequest { get; set; }
		public int QuantityOnStock { get; set; }
		public AssignProductToIssueResult(){}
		public static AssignProductToIssueResult Ok(string message, Guid productId, IReadOnlyList<Pallet> pallets)
		{
			return new AssignProductToIssueResult
			{
				Success = true,
				Message = message,
				ProductId = productId,
				AssignedPallets = pallets
			};
		}

		public static AssignProductToIssueResult Ok(string message)
		{
			return new AssignProductToIssueResult
			{
				Success = true,
				Message = message				
			};
		}
		public static AssignProductToIssueResult Fail(string message)
		{
			return new AssignProductToIssueResult
			{
				Success = false,
				Message = message
			};
		}
		public static AssignProductToIssueResult Fail(string message, Guid productNotAdded)
		{
			return new AssignProductToIssueResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded
			};
		}
		public static AssignProductToIssueResult Fail(
			string message,
			Guid productNotAdded,
			int issueQuantity,
			int onStock)
		{
			return new AssignProductToIssueResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded,
				QuantityRequest = issueQuantity,
				QuantityOnStock = onStock
			};
		}
	}
}
