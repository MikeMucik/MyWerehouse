using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Inventories.Queries.GetInventories
{
	public record GetInventoriesQuery(int PageSize, int PageNumber):IRequest<ListOfInventoryDTO>;	
}
