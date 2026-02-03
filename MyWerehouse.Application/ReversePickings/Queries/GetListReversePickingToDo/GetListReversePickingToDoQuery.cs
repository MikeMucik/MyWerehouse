using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.ReversePickings.DTOs;

namespace MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo
{
	public record GetListReversePickingToDoQuery(int pageSize, int pageNumber, DateOnly Start, DateOnly End): IRequest<ListReversePickingDTO>;
	
}
