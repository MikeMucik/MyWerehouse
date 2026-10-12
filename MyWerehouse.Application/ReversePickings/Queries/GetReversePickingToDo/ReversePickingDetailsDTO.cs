using System;
using MyWerehouse.Application.ReversePickings.DTOs;

namespace MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo
{
	public class ReversePickingDetailsDTO
	{
		public required ReversePickingDTO ReversePickingDTO { get; init; }
		public List<Guid> ListPalletsToAdd {  get; init; } = new List<Guid>();
		public bool CanReturnToSource { get; init; }
		public bool CanAddToExistingPallet { get; init; }
		//public bool PickingPalletCompletlyUnpicking { get; init; }
		public bool AddToNewPallet { get; init; } = true;

	}
}
