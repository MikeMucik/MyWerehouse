using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class VirtualPallet
	{
		public int Id { get; set; }
		public string PalletId { get; set; }
		public Pallet Pallet { get; set; }
		public int IssueInitialQuantity { get; set; }
		public int LocationId { get; set; }
		public Location Location { get; set; }
		public DateTime DateMoved { get; set; }
		//public int IssueId { get; set; }
		public virtual ICollection<Allocation> Allocation { get; set; } = new List<Allocation>();
		[NotMapped]
		public int RemainingQuantity => IssueInitialQuantity - (Allocation?.Sum(a=>a.Quantity) ?? 0);
	}
}
