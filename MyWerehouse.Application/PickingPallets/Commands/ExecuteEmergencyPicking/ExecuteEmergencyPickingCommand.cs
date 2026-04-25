using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteEmergencyPicking
{
	public record ExecuteEmergencyPickingCommand(Guid PalletId, Guid IssueId, string UserId, int RampNumber):IRequest<AppResult<Unit>>;	
}
