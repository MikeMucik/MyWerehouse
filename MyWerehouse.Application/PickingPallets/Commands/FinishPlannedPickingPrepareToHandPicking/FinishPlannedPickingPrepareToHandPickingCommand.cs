using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;

namespace MyWerehouse.Application.PickingPallets.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public record FinishPlannedPickingPrepareToHandPickingCommand()	:IRequest<AppResult<List<PickingTaskDTO>>>;
}
