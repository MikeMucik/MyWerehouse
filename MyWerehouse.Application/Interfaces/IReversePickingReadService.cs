using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking;

namespace MyWerehouse.Application.Interfaces
{
	public interface IReversePickingReadService
	{
		Task<PagedResult<ReversePickingDTO>> GetReversePickingsInDates(DateOnly start, DateOnly end, int pageNumber, int pageSize, CancellationToken ct);
		Task<ReversePickingDTO?> GetReversePicking(Guid reversePickingId, CancellationToken ct);
		Task<List<ReversePickingPalletWithLocationDTO>> GetListPalletsToReversePicking(DateOnly start, DateOnly end, CancellationToken ct);
	}
}
