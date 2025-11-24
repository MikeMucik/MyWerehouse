using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Pallets.Commands.AddPalletToPicking
{
	public record AddPalletToPickingCommand(Issue Issue, int ProductId, DateOnly? BestBefore, string UserId, List<Pallet>? Pallets) : IRequest<VirtualPallet>;
	//public record AddPalletToPickingCommand(Issue Issue, int ProductId, DateOnly? BestBefore, string UserId) : IRequest<VirtualPallet>;

}
