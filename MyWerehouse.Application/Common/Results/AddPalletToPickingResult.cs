using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Common.Results
{
	public class AddPalletToPickingResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public VirtualPallet VirtualPallet { get; set; }
		//public IReadOnlyList<Pallet> RemainingPallets { get; set; }
		public AddPalletToPickingResult() { }
		public static AddPalletToPickingResult Ok(
			VirtualPallet virtualPallet
			//, IReadOnlyList<Pallet> remainingPallets
			)
		{
			return new AddPalletToPickingResult
			{
				Success = true,
				VirtualPallet = virtualPallet,
			//	RemainingPallets = remainingPallets
			};
		}
		public static AddPalletToPickingResult Fail(string message)
		{
			return new AddPalletToPickingResult
			{
				Success = false,
				Message = message
			};
		}
	}
}
