using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class InvalidProductException : DomainException
	{
		public InvalidProductException(int productId)
		: base ($"Produktu o numerze {productId} nie istnieje."){}
	}
}
