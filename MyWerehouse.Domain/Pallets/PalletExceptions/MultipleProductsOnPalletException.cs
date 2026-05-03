using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class MultipleProductsOnPalletException: DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public Guid ProductId { get; }
		public MultipleProductsOnPalletException(Guid palletId, string palletNumber, Guid productId)
			:base($"Product {productId} has multiply to records on pallet, expected one.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
			ProductId = productId;
		}
	}
}
