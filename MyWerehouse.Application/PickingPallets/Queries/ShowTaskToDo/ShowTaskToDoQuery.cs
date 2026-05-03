using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public record ShowTaskToDoQuery(Guid PalletSourceScannedId, DateTime PickingDate, int CurrentPage, int PageSize)
		:IRequest<AppResult<PagedResult<PickingTaskDTO>>>;	
}
