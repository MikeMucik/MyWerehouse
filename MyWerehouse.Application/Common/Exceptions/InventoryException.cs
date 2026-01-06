using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions.BuisnessRuleException;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class InventoryException : BusinessRuleException
	{
		public int ProductId { get; set; }
		//public InventoryException() { }
		public InventoryException(int productId)
			: base($"Brak odpowiedniej ilości produktu o id {productId}")
		{
			ProductId = productId;
		}
		public InventoryException(string message) : base(message) { }
	}
}
