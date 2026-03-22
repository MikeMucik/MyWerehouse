using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Common.Results
{
	public sealed class AssignProductToIssueResult
	{
		public bool Success { get; init; }
		public string Message { get; init; }
		public Guid ProductId { get; init; }		
		public IReadOnlyList<Pallet> AssignedPallets { get; init; }
		public int QuantityRequest { get; set; }
		public int QuantityOnStock { get; set; }
		public AssignProductToIssueResult(){}
		public static AssignProductToIssueResult Ok(string message, IReadOnlyList<Pallet> pallets)
		{
			return new AssignProductToIssueResult
			{
				Success = true,
				Message = message,
				AssignedPallets = pallets
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
