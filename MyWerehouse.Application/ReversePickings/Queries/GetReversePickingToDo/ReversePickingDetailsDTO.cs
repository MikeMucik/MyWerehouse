using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo
{
	public class ReversePickingDetailsDTO
	{
		public ReversePickingDTO ReversePickingDTO { get; init; }
		public List<Pallet> ListPalletsToAdd {  get; init; } = new List<Pallet>();
		public bool CanReturnToSource { get; init; }
		public bool CanAddToExistingPallet { get; init; }
		public bool PickingPalletCompletlyUnpicking { get; init; }
		public bool AddToNewPallet { get; init; } = true;

	}
}
