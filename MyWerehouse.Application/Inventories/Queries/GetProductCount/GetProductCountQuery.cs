using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace MyWerehouse.Application.Inventories.Queries.GetProductCount
{
	public record GetProductCountQuery(int ProductId, DateOnly? BestBefore): IRequest<int>;	
}
