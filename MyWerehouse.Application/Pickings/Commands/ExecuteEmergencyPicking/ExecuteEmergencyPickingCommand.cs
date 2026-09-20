using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.Services;

namespace MyWerehouse.Application.Pickings.Commands.ExecuteEmergencyPicking
{
	public record ExecuteEmergencyPickingCommand(Guid PalletId, Guid IssueId, string UserId, int RampNumber)
		: IRequest<AppResult<ProcessPickingActionResult>>;
}
