using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Domain.Histories.Filters
{
	public class PalletMovementSearchFilter
	{			
		public Guid? ProductId { get; set; }
		public string? ProductName { get; set; }		
		public int? SourceLocationId { get; set; }
		public int? DestinationLocationId { get; set; }		
		public ReasonMovement? Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string? PerformedBy { get; set; } // opcjonalnie: user
		public int? Quantity { get; set; } //+ dodano do palet - usunięto z palety
		public DateTime? MovementDateStart { get; set; }
		public DateTime? MovementDateEnd { get; set; }
	}
}
