using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainPalletException :DomainException
	{
		public string PalletId { get; set; }
		public DomainPalletException(string palletId)
			:base($"Błąd przy zapisie do bazy palety o numerze {palletId}.") 
		{
			PalletId = palletId;
		}
	}
}
