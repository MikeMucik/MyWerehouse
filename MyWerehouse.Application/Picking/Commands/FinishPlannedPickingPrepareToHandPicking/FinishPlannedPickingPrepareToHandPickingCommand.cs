using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.DTOs;

namespace MyWerehouse.Application.Picking.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public record FinishPlannedPickingPrepareToHandPickingCommand(string UserId, DateOnly? Start, DateOnly? End)
		:IRequest<AppResult<List<PickingTaskDTO>>>;
}
