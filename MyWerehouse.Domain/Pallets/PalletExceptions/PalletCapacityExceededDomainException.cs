using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class PalletCapacityExceededDomainException : DomainException
	{
		public Guid PalletId { get; }
		public PalletCapacityExceededDomainException(Guid palletId)
			:base("Quantity is greater than pallet capacity.")
		{
			PalletId = palletId;
		}
	}
}
