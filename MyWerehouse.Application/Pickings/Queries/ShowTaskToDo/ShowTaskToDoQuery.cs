using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.DTOs;

namespace MyWerehouse.Application.Pickings.Queries.ShowTaskToDo
{
	public record ShowTaskToDoQuery(Guid PalletSourceScannedId, DateOnly? PickingDate, int CurrentPage, int PageSize)
		:IRequest<AppResult<PagedResult<PickingTaskDTO>>>;	
}
