using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.ReversePickings.DTOs
{
	public class ReversePickingDetails
	{
		public ReversePickingDTO ReversePickingDTO { get; set; }
		public List<Pallet> ListPalletsToAdd {  get; set; } = new List<Pallet>();
		public bool CanReturnToSource { get; set; }
		public bool CanAddToExistingPallet { get; set; }
		public bool AddToNewPallet { get; set; } = true;

	}
}
