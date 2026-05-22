using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Products.ProductsExceptions
{
	public class WrongLengthProductDomainException : DomainException
	{
		public WrongLengthProductDomainException() : base("Not corect size of length(range: 1-120cm).") { }
	}
}
