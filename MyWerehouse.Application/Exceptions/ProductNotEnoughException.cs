using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Exceptions
{
	public class ProductNotEnoughException : Exception
	{
		public int ProductNotAddedId { get; set; }

		public ProductNotEnoughException(int productNotAddedId)
		: base($"Produkkt o numerze {productNotAddedId} nie został dodany do zlecenia edytuj zlecenie!")
		{
			ProductNotAddedId = productNotAddedId;
		}
	}
}
