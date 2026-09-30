using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Inventories.InventoryExceptions
{
	public class InventoryQuantityDomainException : DomainException
	{
		public Guid ProductId { get; }		
		public InventoryQuantityDomainException(Guid productId)
			: base($"Product ({productId}) quantity below zero - prohibited condition", Common.ValueObject.ErrorType.InternalError)
		{
			ProductId = productId;			
		}
	}
}
