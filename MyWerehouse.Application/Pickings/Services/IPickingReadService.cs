using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Pickings.DTOs;
using MyWerehouse.Application.Pickings.Queries.GetListIssueToPickingTree;
using MyWerehouse.Application.Pickings.Queries.GetListPickingPalletForOperator;
using MyWerehouse.Application.Pickings.Queries.GetListToPickingFlat;
using MyWerehouse.Application.Pickings.Queries.PrepareEmergencyPicking;

namespace MyWerehouse.Application.Picking.Services
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
