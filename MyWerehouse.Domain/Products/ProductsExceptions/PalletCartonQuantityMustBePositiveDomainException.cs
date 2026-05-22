using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Products.ProductsExceptions
{
	public class PalletCartonQuantityMustBePositiveDomainException : DomainException
	{
		public PalletCartonQuantityMustBePositiveDomainException():base("Cartons on pallet must be more than zero.") { }
	}
}
