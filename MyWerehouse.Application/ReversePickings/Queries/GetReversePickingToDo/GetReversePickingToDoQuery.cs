using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo
{
	public record GetReversePickingToDoQuery(Guid PickingTaskId):IRequest<ReversePickingDetails>;
	
}
