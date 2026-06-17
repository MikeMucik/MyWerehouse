using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public sealed class MarkPalletAsLoadedResponeDTO
	{
		public Guid PalletId { get; init; }
		public required string PalletNumber { get; init; }
		public PalletStatus NewStatus { get; init; }
		public DateTime LoadedAt { get; init; }
	}
}
