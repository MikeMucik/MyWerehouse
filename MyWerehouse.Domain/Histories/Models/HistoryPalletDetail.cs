using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Histories.Models
{
	public class HistoryPalletDetail
	{
		public int  Id { get; set; }
		public int HistoryPalletId { get; set; }
		public virtual HistoryPallet HistoryPallet { get; set; }
		public Guid ProductId { get; set; }
		public int Quantity { get; set; } //+/- może zmiana na QuantityChange oczywiście zawsze dodatni 

		public HistoryPalletDetail() { }
		public HistoryPalletDetail(Guid productId, int quantity)
		{
			ProductId = productId;
			Quantity = quantity;
		}	
	}

}
