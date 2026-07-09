using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Inventories.Models
{
	public class Inventory : AggregateRoots
	{		
		public Guid ProductId { get; set; }
		public virtual Product Product { get; set; } = null!;
		public int Quantity { get; set; }				
		public DateTime LastUpdated { get; set; }
				
		public static List<StockItemChange> CreateStockItem(List<Pallet> pallets)
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
