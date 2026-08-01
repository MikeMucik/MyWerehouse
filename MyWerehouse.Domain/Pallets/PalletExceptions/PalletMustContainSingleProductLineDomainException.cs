using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class PalletMustContainSingleProductLineDomainException: DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public PalletMustContainSingleProductLineDomainException(Guid palletId, string palletNumber)
			:base($"Pallet {palletNumber} ({palletId}) must contain exactly one product line.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}		
	}
}
