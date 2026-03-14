using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public record ShowTaskToDoQuery(string PalletSourceScannedId, DateTime PickingDate):IRequest<AppResult<List<PickingTaskDTO>>>;	
}
