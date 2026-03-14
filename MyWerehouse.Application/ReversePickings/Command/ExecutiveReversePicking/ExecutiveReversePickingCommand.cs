using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking
{
	public record ExecutiveReversePickingCommand(Guid TaskReversedId,
		ReversePickingStrategy Strategy, string PickingPalletId, 
		string UserId, List<Pallet>? Pallets):IRequest<AppResult<ReversePickingResult>>;	
}
//List<Pallet> - strategy:ToExist
