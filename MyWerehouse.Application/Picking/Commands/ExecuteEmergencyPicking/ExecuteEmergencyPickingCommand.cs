using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;

namespace MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking
{
	public record ExecuteEmergencyPickingCommand(Guid PalletId, Guid IssueId, string UserId, int RampNumber)
		: IRequest<AppResult<ProcessPickingActionResult>>;
}
