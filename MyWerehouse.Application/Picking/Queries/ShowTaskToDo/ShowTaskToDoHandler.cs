using MediatR;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Picking.DTOs;

namespace MyWerehouse.Application.Picking.Queries.ShowTaskToDo
{
	public class ShowTaskToDoHandler(IPickingReadService pickingReadService, IDateTimeProvider dateTimeProvider)
		: IRequestHandler<ShowTaskToDoQuery, AppResult<PagedResult<PickingTaskDTO>>>
	{
		private readonly IPickingReadService _pickingReadService = pickingReadService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<PagedResult<PickingTaskDTO>>> Handle(ShowTaskToDoQuery request, CancellationToken ct)
		{
			var pickingDate = request.PickingDate ?? _dateTimeProvider.Today;
			var pickingTask = await _pickingReadService.GetPickingTaskForPallet(
				request.PalletSourceScannedId, pickingDate, request.CurrentPage, request.PageSize, ct);
			return AppResult<PagedResult<PickingTaskDTO>>.Success(pickingTask);
		}
	}
}
