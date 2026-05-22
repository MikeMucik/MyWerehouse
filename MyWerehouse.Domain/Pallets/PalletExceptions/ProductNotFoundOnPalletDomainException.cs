using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class ProductNotFoundOnPalletDomainException :DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public Guid ProductId { get; }
		public ProductNotFoundOnPalletDomainException(Guid palletId, string palletNumber, Guid productId)
			:base($"Not found product{productId} on Pallet. Expected one. ")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
			ProductId = productId;
		}
	}
}
