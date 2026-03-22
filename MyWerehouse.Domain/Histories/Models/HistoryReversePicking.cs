using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryReversePicking
	{
		public int Id { get; set; }
		public Guid ReversePickingId { get; set; }
		public Guid? PalletSourceId { get; set; }
		public string? PalletSourceNumber { get; set; }
		public Guid? PalletDestinationId { get; set; }
		public string? PalletDestinationNumber { get; set; }
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus? StatusBefore { get; set; }
		public ReversePickingStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
	}
}
