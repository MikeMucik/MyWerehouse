using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class Inventory
	{		
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public int Quantity { get; set; }				
		public DateTime LastUpdated { get; set; }
	}
}
