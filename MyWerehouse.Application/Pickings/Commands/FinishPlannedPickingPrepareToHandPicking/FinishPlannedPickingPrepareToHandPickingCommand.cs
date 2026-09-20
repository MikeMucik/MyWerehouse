using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.DTOs;

namespace MyWerehouse.Application.Pickings.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public record FinishPlannedPickingPrepareToHandPickingCommand(string UserId, DateOnly? Start, DateOnly? End)
		:IRequest<AppResult<List<PickingTaskDTO>>>;
}
