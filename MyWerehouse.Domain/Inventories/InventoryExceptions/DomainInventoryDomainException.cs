using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Invetories.InventoryExceptions
{
	public class DomainInventoryDomainException : DomainException
	{
		public Guid ProductId { get; }
		public string SKU { get; }
		public DomainInventoryDomainException(Guid productId, string sku)
			: base($"Product {sku}({productId}) quantity below zero - prohibited condition")
		{
			ProductId = productId;
			SKU = sku;
		}
	}
}