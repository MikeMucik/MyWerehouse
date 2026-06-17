using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet
{
	public sealed class ChangeLocationResults
	{
		public bool Success { get; init; }
		public bool RequiresConfirmation { get; init; }
		public string Message { get; init; }
	}
}
