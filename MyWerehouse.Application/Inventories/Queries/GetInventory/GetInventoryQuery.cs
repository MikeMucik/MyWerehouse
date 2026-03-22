using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Inventories.Queries.GetInventory
{
	public record GetInventoryQuery(Guid ProductId):IRequest<InventoryDTO>;	
}
