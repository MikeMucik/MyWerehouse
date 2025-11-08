using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class InventoryException : Exception
	{
		public int ProductId { get; set; }
		public InventoryException() { }
		public InventoryException(int productId)
			: base($"Brak odpowiedniej ilości produktu o id {productId}")
		{
			ProductId = productId;
		}
		public InventoryException(string message) : base(message) { }
	}
}
