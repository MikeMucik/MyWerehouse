using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.AllocationModels;

namespace MyWerehouse.Application.PickingPallets.Commands.DoPicking
{
	public record DoPickingCommand(AllocationDTO AllocationDTO, string UserId):IRequest<PickingResult>;	
}
