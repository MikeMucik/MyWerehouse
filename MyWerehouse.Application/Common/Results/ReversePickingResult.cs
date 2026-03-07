using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Common.Results
{
	public class ReversePickingResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public int ProductId { get; set; }
		public string PalletId { get; set; }
		public List<Pallet> PalletWithAddedProduct { get; set; }
		public ReversePickingResult() { }
		public static ReversePickingResult Ok(string message, int productId, string palletId)
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
		public static ReversePickingResult Ok(string message, List<Pallet> palletWithAddedProduct)
		{
			return new ReversePickingResult
			{
				Success = true,
				Message = message,
				PalletWithAddedProduct = palletWithAddedProduct
			};
		}
		//public static ReversePickingResult Fail(string message, int productId, string palletId)
		//{
		//	return new ReversePickingResult
		//	{
		//		Success = false,
		//		Message = message,
		//		ProductId = productId,
		//		PalletId = palletId
		//	};
		//}
		public static ReversePickingResult Fail(string message)
		{
			return new ReversePickingResult
			{
				Success =false,
				Message = message				
			};
		}
	}
}
