using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.ReduceAllocation
{
	public record ReduceAllocationCommand(Issue Issue, int ProductId, int Quantity, string UserId):IRequest<Unit>;
	
}
