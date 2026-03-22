using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Histories.Models
{
	public class PalletMovement
	{
		public int Id { get; set; }
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; }
		public int? SourceLocationId { get; set; }
		public string? SourceLocationSnapShot { get; set; }
		public int? DestinationLocationId { get; set; }
		public string? DestinationLocationSnapShot { get; set; }
		public ReasonMovement Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string PerformedBy { get; set; } 		
		public virtual ICollection<PalletMovementDetail> PalletMovementDetails { get; set; } = new List<PalletMovementDetail>();
		public DateTime MovementDate { get; set; }
		public PalletStatus PalletStatus { get; set; } 
	}
}
