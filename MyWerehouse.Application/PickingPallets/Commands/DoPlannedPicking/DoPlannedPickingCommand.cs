using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.PickingTaskModels;

namespace MyWerehouse.Application.PickingPallets.Commands.DoPicking
{
	public record DoPlannedPickingCommand(PickingTaskDTO PickingTaskDTO, string UserId):IRequest<PickingResult>;	
}
