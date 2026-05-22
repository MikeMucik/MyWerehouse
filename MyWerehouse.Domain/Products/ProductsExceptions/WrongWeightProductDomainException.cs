using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Products.ProductsExceptions
{
public class WrongWeightProductDomainException: DomainException
	{
		public WrongWeightProductDomainException() : base("Not corect weight (range: 1-50000g).") { }
	}
}
