using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Histories.Models
{
	public class PalletMovementDetail
	{
		public int  Id { get; set; }
		public int PalletMovementId { get; set; }
		public virtual PalletMovement PalletMovement { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; } //+/- może zmiana na QuantityChange oczywiście zawsze dodatni 

		public PalletMovementDetail() { }
		public PalletMovementDetail(Guid productId, int quantity)
		{
			ProductId = productId;
			Quantity = quantity;
		}	
	}

}
