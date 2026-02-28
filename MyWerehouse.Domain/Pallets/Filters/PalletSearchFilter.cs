using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Pallets.Filters
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
		public Guid? ReceiptId { get; set; }//
		public Guid? IssueId { get; set; }//
	}
}
