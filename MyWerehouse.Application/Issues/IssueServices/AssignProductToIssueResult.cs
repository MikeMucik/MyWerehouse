using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public sealed class AssignProductToIssueResult
	{
		public bool Success { get; init; }
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public string Message { get; set; } = string.Empty;
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public Guid? ProductId { get; init; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? SKU { get; init; } 

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public IReadOnlyList<Pallet>? AssignedPallets { get; init; } 

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? QuantityRequest { get; init; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? QuantityOnStock { get; init; }
		public AssignProductToIssueResult() { }
		public static AssignProductToIssueResult Ok(
			Guid issueId,
			int issueNumber,
			string message,
			Guid productId,
			string sku,
			IReadOnlyList<Pallet> pallets,
			int quantityRequest,
			int quantityOnStock)
		{
			return new AssignProductToIssueResult
			{
				Success = true,
				IssueId = issueId,
				IssueNumber = issueNumber,
				Message = message,
				ProductId = productId,
				SKU = sku,
				AssignedPallets = pallets,
				QuantityRequest = quantityRequest,
				QuantityOnStock = quantityOnStock
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
		public static AssignProductToIssueResult Fail(
			Guid issueId,
			int issueNumber,
			string message,
			Guid productNotAdded,
			int quantityRequest)
		{
			return new AssignProductToIssueResult
			{
				Success = false,
				IssueId = issueId,
				IssueNumber = issueNumber,
				Message = message,
				ProductId = productNotAdded,
				QuantityRequest = quantityRequest
			};
		}
		public static AssignProductToIssueResult Fail(
			Guid issueId,
			int issueNumber,
			string message,
			Guid productNotAdded,
			string sku,
			int issueQuantity,
			int onStock)
		{
			return new AssignProductToIssueResult
			{
				Success = false,
				IssueId = issueId,
				IssueNumber = issueNumber,
				Message = message,
				ProductId = productNotAdded,
				SKU = sku,
				QuantityRequest = issueQuantity,
				QuantityOnStock = onStock
			};
		}
	}
}
