using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundProductException : NotFoundException
	{
		public NotFoundProductException(int productId)
		: base ($"Produkt o numerze {productId} nie istnieje."){}
	}
}
