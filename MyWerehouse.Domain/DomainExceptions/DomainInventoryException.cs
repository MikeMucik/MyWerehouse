using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainInventoryException :DomainException
	{
		public int ProductId { get; set; }
		public string Message { get; set; }
		public DomainInventoryException(int productId, string message)
			:base ($"Stan producktu {productId} pniżej zera - stan niedozwolony.")
		{
			ProductId = productId;
			Message = message;
		}
		public DomainInventoryException(int productId)
			: base($"Stan producktu {productId} pniżej zera - stan niedozwolony.")
		{
			ProductId = productId;
		}
	}
}
