using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class PalletMovement
	{
		public int Id { get; set; }
		public string PalletId { get; set; }
		public virtual Pallet Pallet { get; set; }		
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public int LocationId { get; set; }
		public virtual Location Location { get; set; }			  
		public ReasonMovement Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string? PerformedBy { get; set; } // opcjonalnie: user
		public int Quantity { get; set; } //+ dodano do palet - usunięto z palety
		public DateTime MovementDate { get; set; }
	}
}
