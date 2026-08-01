using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pallets.PalletExceptions
{
	public class NotCorrectDateBestBeforeDomainException :DomainException
	{
		public Guid PalletId { get; }
		public string Palletumber { get; }
		public NotCorrectDateBestBeforeDomainException(Guid palletId, string palletumber):
			base($"Product on pallet {palletId}, {palletumber} has wrond bestBefore Date")
		{
			PalletId = palletId;
			Palletumber = palletumber;
		}
	}
}
