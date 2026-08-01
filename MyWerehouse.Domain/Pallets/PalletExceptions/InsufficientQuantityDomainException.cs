using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class InsufficientQuantityDomainException :DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public InsufficientQuantityDomainException(Guid palletId, string palletNumber)
			: base("Insufficient/wrong quantity.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
	}
}
