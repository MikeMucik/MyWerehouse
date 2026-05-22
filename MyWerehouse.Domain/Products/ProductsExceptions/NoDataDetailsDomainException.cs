using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Products.ProductsExceptions
{
public class NoDataDetailsDomainException :DomainException
	{
		public NoDataDetailsDomainException():base("Inavalid data details for product.") { }
	}
}
