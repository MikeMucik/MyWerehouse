using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Invetories.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Invetories.Models
{
	public class Inventory : AggregateRoots
	{		
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public int Quantity { get; set; }				
		public DateTime LastUpdated { get; set; }
				
		public List<StockItemChange> CreateStockItem(List<Pallet> pallets)
		{
			var list = new List<StockItemChange>();
			foreach (var pallet in pallets)
			{
				list = [.. pallet.ProductsOnPallet
					.GroupBy(p=>p.ProductId)
					.Select(g=> new StockItemChange(g.Key, g.Sum(x=>x.Quantity)))];				
			}
			return list;
		}
	}
	
}
