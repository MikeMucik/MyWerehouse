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
		public virtual HistoryPallet HistoryPallet { get; set; } = null!;
		public Guid ProductId { get; set; }
		public int QuantityChange { get; set; } 

		public HistoryPalletDetail() { }
		public HistoryPalletDetail(Guid productId, int quantity)
		{
			ProductId = productId;
			QuantityChange = quantity;
		}	
	}

}
