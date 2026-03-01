using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryPicking
	{
		public int Id { get; set; }
		public Guid PickingTaskId { get; set; }		
		public string PalletId { get; set; }
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public int ProductId { get; set; }
		public int QuantityAllocated { get; set; }   // ile system przydzielił
		public int QuantityPicked { get; set; }      // ile picker potwierdził
		public PickingStatus StatusBefore { get; set; }
		public PickingStatus StatusAfter { get; set; }		
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
	}
}
