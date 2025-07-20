using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class PalletSearchFilter
	{
		public int? ProductId { get; set; }
		public string? ProductName { get; set; }
		public int? LocationId { get; set; }
		public PalletStatus? PalletStatus { get; set; }
		public int? ClientIdIn { get; set; }
		public int? ClientIdOut { get; set; }
		public DateOnly? BestBefore {  get; set; }
		public DateOnly? BestBeforeTo {  get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string? ReceiptUser {  get; set; }
		public string? IssueUser { get; set; }
		public int? ReceiptId { get; set; }//
		public int? IssueId { get; set; }//
	}
}
