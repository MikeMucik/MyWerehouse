using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.IssueServices
{
	public class ComparePlanToPreparedResult
	{
		public bool Success { get; init; }
		public string Message { get; init; }
		public Guid ProductId { get; init; }
		public string SKU { get; init; }
		public int QuantityRequest { get; init; }
		public int QuantityPrepared { get; init; }
		public ComparePlanToPreparedResult() { }
		public static ComparePlanToPreparedResult Ok(string message, Guid productId, string sku)
		{
			return new ComparePlanToPreparedResult
			{
				Success = true,
				Message = message,
				ProductId = productId,
				SKU = sku
			};
		}
		public static ComparePlanToPreparedResult Fail(string message,
			Guid productId, string sku, int quantityRequest, int quantityPrepared)
		{
			return new ComparePlanToPreparedResult
			{
				Success = false,
				Message = message,
				ProductId = productId,
				SKU = sku,
				QuantityRequest = quantityRequest,
				QuantityPrepared = quantityPrepared
			};
		}
		public static ComparePlanToPreparedResult Fail(string message)
		{
			return new ComparePlanToPreparedResult
			{
				Success = false,
				Message = message
			};
		}
		public static ComparePlanToPreparedResult Fail(string message, Guid productId, string sku)
		{
			return new ComparePlanToPreparedResult
			{
				Success = false,
				Message = message,
				ProductId = productId,
				SKU = sku
			};
		}
	}
}
