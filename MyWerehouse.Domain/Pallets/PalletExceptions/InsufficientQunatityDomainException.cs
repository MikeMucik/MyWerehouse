using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class InsufficientQunatityDomainException :DomainException
	{
		public Guid PalletId { get; }
		public InsufficientQunatityDomainException(Guid palletId)
			: base("Insufficient/wrong quantity.")
		{
			PalletId = palletId;
		}
	}
}