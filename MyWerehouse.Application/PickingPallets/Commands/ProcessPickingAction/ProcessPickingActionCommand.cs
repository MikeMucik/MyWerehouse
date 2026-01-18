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

namespace MyWerehouse.Application.PickingPallets.Commands.ProcessPickingAction
{
	public record ProcessPickingActionCommand(Pallet SourcePallet,
		Issue Issue, int ProductId, int QuantityToPick,
		string UserId, Allocation Allocation,
		PickingCompletion PickingCompletion):IRequest<ProcessPickingActionResult>;	
}
