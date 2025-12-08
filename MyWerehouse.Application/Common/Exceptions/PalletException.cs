using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class PalletException : Exception
	{
		public string? PalletId { get; }
		
		public PalletException() { }
		public PalletException(string palletId, string? message = null)
			: base(message?? $"Paleta o numerze {palletId} nie została znaleziona.")
		{
			PalletId = palletId;
		}
		public PalletException(string message) : base(message) { }
		public PalletException(string message, Exception inner) : base(message, inner) { }
	}
}
