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
		public string Message { get; set; } = string.Empty;
		public Guid ProductId { get; init; }
		public string SKU { get; init; } = string.Empty;
		public IReadOnlyList<Pallet> AssignedPallets { get; init; } = [];
		public int QuantityRequest { get; init; }
		public int QuantityOnStock { get; init; }
		public AssignProductToIssueResult() { }
		public static AssignProductToIssueResult Ok(string message, Guid productId, string sku, IReadOnlyList<Pallet> pallets)
		{
			return new AssignProductToIssueResult
			{
				Success = true,
				Message = message,
				ProductId = productId,
				SKU = sku,
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
				ProductId = productNotAdded,				
			};
		}
		public static AssignProductToIssueResult Fail(
			string message,
			Guid productNotAdded,
			string sku,
			int issueQuantity,
			int onStock)
		{
			return new AssignProductToIssueResult
			{
				Success = false,
				Message = message,
				ProductId = productNotAdded,
				SKU = sku,
				QuantityRequest = issueQuantity,
				QuantityOnStock = onStock
			};
		}
	}
}
