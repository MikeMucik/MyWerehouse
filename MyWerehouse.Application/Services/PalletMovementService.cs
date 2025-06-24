using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Services
{
	public class PalletMovementService : IPalletMovementService
	{
		private readonly IPalletMovementRepo _palletMovementRepo;

		public PalletMovementService(IPalletMovementRepo palletMovementRepo)
		{
			_palletMovementRepo = palletMovementRepo;
		}

		//public void RegisterPalletMovement(string palletId, int productId, int locationId, int quantity, ReasonMovement reason, string performedBy)
		//{
		//	var movement = new PalletMovement
		//	{
		//		PalletId = palletId,
		//		ProductId = productId,
		//		LocationId = locationId,
		//		Quantity = quantity,
		//		Reason = reason,
		//		PerformedBy = performedBy,
		//		MovementDate = DateTime.UtcNow
		//	};

		//	_palletMovementRepo.AddPalletMovement(movement);
		//}
	}
}
