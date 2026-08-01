using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;

namespace MyWerehouse.Application.Picking.Commands.DoPlannedPicking
{
	public record DoPlannedPickingCommand(
		Guid PickingTaskId, Guid SourcePalletId, int PickedQuantity, int RampNumber, string UserId)
		:IRequest<AppResult<ProcessPickingActionResult>>;	
}
