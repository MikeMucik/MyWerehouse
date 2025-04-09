using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class ProductOnPallet
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public string PalletId { get; set; }
		public virtual Pallet Pallet { get; set; }		
		public int Quantity { get; set; }
		public DateTime DateAdded { get; set; }
		public DateOnly? BestBefore { get; set; } // Może być null, jeśli produkt nie ma daty ważności
		
	}
}
