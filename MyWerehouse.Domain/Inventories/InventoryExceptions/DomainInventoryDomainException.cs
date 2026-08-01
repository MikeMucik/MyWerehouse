using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Inventories.InventoryExceptions
{
	public class DomainInventoryDomainException : DomainException
	{
		public Guid ProductId { get; }		
		public DomainInventoryDomainException(Guid productId)
			: base($"Product ({productId}) quantity below zero - prohibited condition")
		{
			ProductId = productId;			
		}
	}
}
