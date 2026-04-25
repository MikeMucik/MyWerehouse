using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class InvalidPalletStatusException : DomainException
	{
		public Guid PalletId { get; }
		public InvalidPalletStatusException(Guid palletId)
		: base("Pallet has wrong status.")
		{
			PalletId = palletId;
		}
	}
}
