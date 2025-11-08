using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class ProductException : Exception
	{
		public int? ProductId { get; set; }

		public ProductException(int productId)
			: base($"Produkt o numerze {productId} nie został dodany do zlecenia edytuj zlecenie!")
		{
			ProductId = productId;
		}
		public ProductException(string message) : base(message) { }

		public ProductException(string message, Exception inner) : base(message, inner) { }
	}
}