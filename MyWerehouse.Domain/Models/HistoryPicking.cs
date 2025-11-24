using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class HistoryPicking
	{
		public int Id { get; set; }
		public int? AllocationId { get; set; }// ? dla update
		//[JsonIgnore] // Ignoruj przy serializacji
		public Allocation Allocation { get; set; }
		public string PalletId { get; set; }
		public Pallet Pallet { get; set; }
		public int IssueId { get; set; }
		public Issue Issue { get; set; }
		public int ProductId { get; set; }
		public int QuantityAllocated { get; set; }   // ile system przydzielił
		public int QuantityPicked { get; set; }      // ile picker potwierdził
		public PickingStatus StatusBefore { get; set; }
		public PickingStatus StatusAfter { get; set; }		
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
	}
}
