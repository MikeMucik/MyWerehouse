using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class InvalidPalletStatusDomainException : DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public InvalidPalletStatusDomainException(Guid palletId, string palletNumber)
			: base($"Pallet {palletId}, {palletNumber} has wrong status. Operation stopped.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
	}
}
