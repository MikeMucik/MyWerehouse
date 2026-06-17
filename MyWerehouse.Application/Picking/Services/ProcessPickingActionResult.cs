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
		public string Message { get; init; }
		public Guid PalletId { get; init; }
		public string PalletNumber { get; init; }
		public ProcessPickingActionResult() { }
		public static ProcessPickingActionResult Ok(Guid palletId
			, string palletNumber
			)
		{
			return new ProcessPickingActionResult
			{
				Success = true,
				PalletId = palletId,
				PalletNumber = palletNumber
			};
		}
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