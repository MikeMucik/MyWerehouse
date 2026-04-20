using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions.PalletExceptions
{
	public class TwoDateOneProductOnPalletException :DomainException
	{
		public Guid PalletId { get; }
		public TwoDateOneProductOnPalletException(Guid palletId)
			: base("Cannot mix different expiration dates on same pallet.")
		{
			PalletId = palletId;
		}
	}
}
