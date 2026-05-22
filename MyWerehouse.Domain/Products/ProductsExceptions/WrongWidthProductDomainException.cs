using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Products.ProductsExceptions
{
	public	class WrongWidthProductDomainException : DomainException
	{
		public WrongWidthProductDomainException() : base("Not corect size of width (range: 1-120cm).") { }
	}
}
