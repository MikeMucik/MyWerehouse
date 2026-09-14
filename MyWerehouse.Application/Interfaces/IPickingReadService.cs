using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree;
using MyWerehouse.Application.Picking.Queries.GetListPickingPalletForOperator;
using MyWerehouse.Application.Picking.Queries.GetListToPickingFlat;
using MyWerehouse.Application.Picking.Queries.PrepareEmergencyPicking;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPickingReadService
	{
		Task<List<PickingGuideLineDTO>> GetPickingTaskFlat(DateOnly startDate, DateOnly endDate, CancellationToken ct);
		Task<PagedResult<PickingPalletWithLocationDTO>> GetSourcePalletList(DateOnly startDate, DateOnly endDate,int pageNumber, int pageSize, CancellationToken ct);
		Task<List<ProductToIssueDTO>> GetProductToIssueList(DateOnly startDate, DateOnly endDate, CancellationToken ct);
		Task<List<IssueOptions>> GetProperpickingTask(Guid productId, DateOnly start, DateOnly end, CancellationToken ct);
		Task<PagedResult<PickingTaskDTO>> GetPickingTaskForPallet(Guid palletId, DateOnly pickingDate,int pageNumber, int pageSize, CancellationToken ct);
	}
}
