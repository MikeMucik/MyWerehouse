using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainProductOnPalletException :DomainException
	{
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; }
		public DomainProductOnPalletException(Guid palletId, string palletNumber)
			: base($"Brak produktów na palecie {palletNumber}.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
	}
}
