using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Warehouse.Models;


namespace MyWerehouse.Domain.Picking.Models
{
	public class VirtualPallet
	{
		public int Id { get; set; }
		public string PalletId { get; set; }
		public Pallet Pallet { get; set; }
		public int InitialPalletQuantity { get; set; }
		public int LocationId { get; set; }
		public Location Location { get; set; }
		public DateTime DateMoved { get; set; }
		public virtual ICollection<PickingTask> PickingTasks { get; set; } //= new List<PickingTask>();
		public virtual ICollection<HistoryPicking> HistoryPicking { get; set; } = new List<HistoryPicking>();
		[NotMapped]
		public int RemainingQuantity => InitialPalletQuantity - (PickingTasks?.Sum(a=>a.Quantity) ?? 0);		
	}
}
