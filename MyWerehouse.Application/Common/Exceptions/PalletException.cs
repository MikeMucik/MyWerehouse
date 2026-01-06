using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions.BuisnessRuleException;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class PalletException : BusinessRuleException
	{
		public string? PalletId { get; }
		
		//public PalletException() { }
		public PalletException(string palletId, string? message = null)
			: base(message?? $"Paleta o numerze {palletId} nie została znaleziona.")
		{
			PalletId = palletId;
		}
		public PalletException(string message) : base(message) { }
		//public PalletException(string message, Exception inner) : base(message, inner) { }
	}
}
