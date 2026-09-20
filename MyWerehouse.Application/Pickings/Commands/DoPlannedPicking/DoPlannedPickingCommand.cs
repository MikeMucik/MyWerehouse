using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.Services;

namespace MyWerehouse.Application.Pickings.Commands.DoPlannedPicking
{
	public record DoPlannedPickingCommand(
		Guid PickingTaskId, Guid SourcePalletId, int PickedQuantity, int RampNumber, string UserId)
		:IRequest<AppResult<ProcessPickingActionResult>>;	
}
