using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class Inventory
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public int Quantity { get; set; }		
		public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
		public DateTime LastUpdated { get; set; }
	}
}
