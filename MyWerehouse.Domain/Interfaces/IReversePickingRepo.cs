using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IReversePickingRepo
	{
		void AddReversePicking(ReversePickingTask reversePicking);
		Task<ReversePickingTask?> GetReversePickingAsync(Guid reversePickingId, CancellationToken ct);
		IQueryable<ReversePickingTask> GetReversePickings();
		Task<bool> ExistsForPickingPalletAsync(Guid palletId, CancellationToken ct);
		Task<List<Guid>> GetPalletsIdsByDate(DateOnly start, DateOnly end, CancellationToken ct);
	}
}
