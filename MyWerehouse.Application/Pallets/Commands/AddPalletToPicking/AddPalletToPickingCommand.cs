using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Pallets.Commands.AddPalletToPicking
{
	public record AddPalletToPickingCommand(Issue Issue, int ProductId, DateOnly? BestBefore, string UserId, List<Pallet>? Pallets) 
		: IRequest<AddPalletToPickingResult>;
}
//IReadOnly
