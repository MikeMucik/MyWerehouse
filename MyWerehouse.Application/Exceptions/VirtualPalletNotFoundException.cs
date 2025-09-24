using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Exceptions
{
	public class VirtualPalletNotFoundException : Exception
	{
		public int VirtualId { get; }

		public VirtualPalletNotFoundException(int virtualPalletId)
			: base($"Zamówienie o numerze {virtualPalletId} nie zostało znalezione.")
		{
			VirtualId = virtualPalletId;
		}
		public VirtualPalletNotFoundException(string message) : base(message) { }

		public VirtualPalletNotFoundException(string message, Exception inner) : base(message, inner) { }

	}
}
