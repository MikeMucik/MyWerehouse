using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Invetories.InventoryExceptions
{
	public class DomainInventoryException : DomainException
	{
		public Guid ProductId { get; set; }
		public DomainInventoryException(Guid productId)
			: base($"Product {productId} quantity below zero - prohibited condition")
		{
			ProductId = productId;
		}
	}
}
