using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions.PalletExceptions
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
