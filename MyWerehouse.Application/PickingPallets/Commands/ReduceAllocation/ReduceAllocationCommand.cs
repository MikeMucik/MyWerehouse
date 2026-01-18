using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.ReduceAllocation
{
	public record ReduceAllocationCommand(Issue Issue, int ProductId, int Quantity, string UserId):IRequest<List<Allocation>>;
	
}
