using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pickings.DTOs;

namespace MyWerehouse.Application.Pickings.Queries.PrepareEmergencyPicking
{
	public record PrepareEmergencyPickingQuery(Guid PalletId, DateOnly Start, DateOnly End)
		:IRequest<AppResult<PrepareCorrectedPickingResult>>;	
}
