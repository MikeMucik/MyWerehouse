using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Exceptions
{
	public class PalletNotFoundException : Exception
	{
		public string? PalletId { get; }
		
		public PalletNotFoundException() { }
		public PalletNotFoundException(string palletId, string? message = null)
			: base(message?? $"Paleta o numerze {palletId} nie została znaleziona.")
		{
			PalletId = palletId;
		}
		public PalletNotFoundException(string message) : base(message) { }
		public PalletNotFoundException(string message, Exception inner) : base(message, inner) { }
	}
}
