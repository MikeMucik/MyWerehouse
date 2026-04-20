using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions.PalletExceptions
{
	public class AlreadyAssignedException :DomainException
	{
		public Guid PalletId { get; }
		public AlreadyAssignedException(Guid palletId):
			base("Pallet already assigned.")
		{
			PalletId = palletId;
		}
	}
}
