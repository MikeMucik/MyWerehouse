using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Products.ProductsExceptions
{
	public class WrongHeightProductDomainException : DomainException
	{
		public WrongHeightProductDomainException() : base("Not corect size of height (range: 1-220cm).") { }
	}
}
