using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class PalletIsNotForReverseDomainException :DomainException
	{
		public Guid Id { get; }
		public string PalletNumber { get; }
		public PalletIsNotForReverseDomainException(Guid id, string palletNumber)
			:base ($"Pallet {id} {palletNumber} is not to ReversePicking")
		{
			Id = id;
			PalletNumber = palletNumber;
		}
	}
}
