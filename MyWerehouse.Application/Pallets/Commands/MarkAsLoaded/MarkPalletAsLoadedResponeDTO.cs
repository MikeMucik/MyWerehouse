using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkPalletAsLoadedResponeDTO
	{
		public Guid PalletId { get; set; }
		public required string PalletNumber { get; set; }
		public PalletStatus NewStatus { get; set; }
		public DateTime LoadedAt { get; set; }
	}
}
