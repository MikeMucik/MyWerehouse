using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class PalletMovementDetail
	{
		public int  Id { get; set; }
		public int PalletMovementId { get; set; }
		public virtual PalletMovement PalletMovement { get; set; }
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public int Quantity { get; set; } //+/- może zmiana na QuantityChange oczywiście zawsze dodatni 
	}
}
