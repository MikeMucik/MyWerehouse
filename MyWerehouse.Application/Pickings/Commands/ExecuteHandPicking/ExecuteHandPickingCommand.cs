using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.Services;

namespace MyWerehouse.Application.Pickings.Commands.ExecuteHandPicking
{
	public record ExecuteHandPickingCommand(Guid PalletIdSource,
		Guid IssueId, int PickedQuantity, string UserId, int RampNumber)
		:IRequest<AppResult<ProcessPickingActionResult>>;
}
