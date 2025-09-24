using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainVirtualPalletException : Exception
	{
		public string PalletId { get; set; }

		public DomainVirtualPalletException(string message) : base(message) { }
	}
}
