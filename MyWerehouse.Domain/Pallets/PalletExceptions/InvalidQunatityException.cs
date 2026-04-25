using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class InvalidQunatityException : DomainException
	{
		public Guid PalletId { get; }
		public InvalidQunatityException(Guid palletId)
			: base("Quantity must be greater than zero.")
		{
			PalletId = palletId;
		}
	}
}
