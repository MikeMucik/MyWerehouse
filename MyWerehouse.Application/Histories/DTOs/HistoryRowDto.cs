using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public record HistoryRowDto
	{
		public DateTime Date { get; init; }
		public string Action { get; init; }
		public string EntityType { get; init; } // Pallet / Issue / Product
		public string EntityId { get; init; }
		public string PerformedBy { get; init; }
		public string Description { get; init; }
	}

}
//GET / history / pallet ? palletId = ...
//GET / history / issue ? issueId = ...
//GET / history / product ? productId = ...
//GET / history / picking ? pickingPalletId = ...