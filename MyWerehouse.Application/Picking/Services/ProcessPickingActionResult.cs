using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Services
{
	public sealed class ProcessPickingActionResult
	{
		public bool Success { get; init; }

		public bool NewPalletCreated { get; init; }
		public string Message { get; set; }

		public int RequestedQuantity { get; init; }
		public int PickedQuantity { get; init; }
		public int MissingQuantity { get; init; }

		public Guid PalletId { get; init; }
		public string PalletNumber { get; init; }
		public ProcessPickingActionResult() { }
		public static ProcessPickingActionResult Ok(Guid palletId
			, string palletNumber, string message
			)
		{
			return new ProcessPickingActionResult
			{
				Success = true,
				NewPalletCreated = false,
				PalletId = palletId,
				PalletNumber = palletNumber,
				Message = message
			};
		}
		public static ProcessPickingActionResult OkWithNewPallet(Guid palletId
			, string palletNumber, string message
			)
		{
			return new ProcessPickingActionResult
			{
				Success = true,
				NewPalletCreated = true,
				PalletId = palletId,
				PalletNumber = palletNumber,
				Message = message
			};
		}

		//public static ProcessPickingActionResult OkPartial(bool newPalletCreted, Guid palletId, 
		//	string palletNubmer, string message, int requestQuantity,
		//	int pickedQuantity, int missingQuantity)
		//{
		//	return new ProcessPickingActionResult
		//	{
		//		Success = true,
		//		NewPalletCreated = newPalletCreted,
		//		Message = message,
		//		PalletId = palletId,
		//		PalletNumber = palletNubmer,
		//		RequestedQuantity = requestQuantity,
		//		PickedQuantity = pickedQuantity,
		//		MissingQuantity = missingQuantity
		//	};
		//}

		public static ProcessPickingActionResult Fail(string message)
		{
			return new ProcessPickingActionResult
			{
				Success = false,
				Message = message
			};
		}
	}
}