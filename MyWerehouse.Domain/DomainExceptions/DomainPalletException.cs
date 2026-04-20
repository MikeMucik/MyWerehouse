using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainPalletException :DomainException
	{
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; }
		public DomainPalletException(Guid palletId,string palletNumber)
			:base($"Błąd przy zapisie do bazy palety o numerze {palletNumber}.") 
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
		public DomainPalletException()
			: base($"Błąd przy zapisie do bazy.")
		{			
		}
	}
}
