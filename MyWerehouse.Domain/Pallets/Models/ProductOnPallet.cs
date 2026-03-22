using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class ProductOnPallet
	{
		public int Id { get; set; }
		public Guid ProductId { get; set; }
		public virtual Product Product { get; set; }
		public Guid PalletId { get; set; }
		//public string PalletNumber { get; set; }
		public virtual Pallet Pallet { get; set; }		
		public int Quantity { get; set; }
		public DateTime DateAdded { get; set; }
		public DateOnly? BestBefore { get; set; } 
	}
}
