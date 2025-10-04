using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


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
		public virtual ICollection<Allocation> Allocations { get; set; } //= new List<Allocation>();
		public virtual ICollection<HistoryPicking> HistoryPicking { get; set; } = new List<HistoryPicking>();
		[NotMapped]
		public int RemainingQuantity => IssueInitialQuantity - (Allocations?.Sum(a=>a.Quantity) ?? 0);		
	}
}
