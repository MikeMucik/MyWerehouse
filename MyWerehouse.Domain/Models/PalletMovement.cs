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
		public int? SourceLocationId { get; set; }
		public virtual Location SourceLocation { get; set; }
		public int? DestinationLocationId { get; set; }
		public virtual Location DestinationLocation { get; set; }			  
		public ReasonMovement Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string? PerformedBy { get; set; } // opcjonalnie: user		
		public virtual ICollection<PalletMovementDetail> PalletMovementDetails { get; set; }
		public DateTime MovementDate { get; set; }
	}
}
