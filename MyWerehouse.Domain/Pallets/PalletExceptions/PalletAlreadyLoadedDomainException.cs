using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class PalletAlreadyLoadedDomainException : DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public PalletAlreadyLoadedDomainException(Guid palletId, string palletNumber)
			   : base($"Pallet {palletId}, {palletNumber} is already loaded.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
	}
}
