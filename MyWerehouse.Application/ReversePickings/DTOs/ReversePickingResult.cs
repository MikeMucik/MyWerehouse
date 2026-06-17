using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.DTOs
{
	public sealed class ReversePickingResult
	{
		public bool Success { get; init; }
		public string Message { get; init; }
		public Guid ProductId { get; init; }
		public Guid? PalletId { get; init; }
		public string? PalletNumber { get; init; }
		public List<PalletProductQuantityDTO> PalletWithAddedProduct { get; init; }
		public ReversePickingResult() { }
		public static ReversePickingResult Ok(string message, Guid productId, Guid? palletId, string? palletNumber)
		{
			return new ReversePickingResult
			{
				Success = true,
				Message = message,
				ProductId = productId,
				PalletId = palletId,
				PalletNumber = palletNumber
			};
		}
		public static ReversePickingResult Ok(string message, Guid productId, Guid? palletId)
		{
			return new ReversePickingResult
			{
				Success = true,
				Message = message,
				ProductId = productId,
				PalletId = palletId
			};
		}
		public static ReversePickingResult Ok()
		{
			return new ReversePickingResult
			{
				Success = true,
			};
		}
		public static ReversePickingResult Ok(string message, List<PalletProductQuantityDTO> palletWithAddedProduct)
		{
			return new ReversePickingResult
			{
				Success = true,
				Message = message,
				PalletWithAddedProduct = palletWithAddedProduct
			};
		}

		public static ReversePickingResult Fail(string message)
		{
			return new ReversePickingResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
