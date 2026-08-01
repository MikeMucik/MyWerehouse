using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Inventories.InventoryExceptions;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Inventories.Models
{
	public class Inventory : AggregateRoots
	{
		public Guid ProductId { get; private set; }
		public virtual Product Product { get; set; } = null!;
		public int Quantity { get; private set; }
		public DateTime LastUpdated { get; set; }
		public Inventory() { }

		private Inventory(Guid productId, int quantity, DateTime dateTime)
		{
			ProductId = productId;
			if (quantity < 0)
			{
				throw new DomainInventoryDomainException(ProductId);
			}
			Quantity = quantity;
			LastUpdated = dateTime;
		}		
		public static Inventory CreateStockItem(Guid productId, int quantity, DateTime dateTime)
			=> new Inventory(productId, quantity, dateTime);
		public void ApplyChangeInInventory(int quantity, DateTime dateTime)
		{
			var newQuantity = Quantity + quantity;

			if (newQuantity < 0)
			{				
				throw new DomainInventoryDomainException(ProductId);
			}
			LastUpdated = dateTime;
			Quantity = newQuantity;
		}
	}
}
