using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking
{
	public record ExecuteReversePickingCommand(Guid TaskReversedId,
		ReversePickingStrategy Strategy, Guid PickingPalletId, 
		string UserId,
		//List<Pallet>? Pallets,
		List<Guid>? PalletsIds,
		int? RampNumber)
		:IRequest<AppResult<ReversePickingResult>>;	
}
//List<Pallet> - strategy:ToExist
