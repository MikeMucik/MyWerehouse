using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Pickings.Commands.ClosePickingPallet
{
	public record ClosePickingPalletCommand(Guid PalletId, Guid IssueId, string UserId) : IRequest<AppResult<Unit>>;
}
