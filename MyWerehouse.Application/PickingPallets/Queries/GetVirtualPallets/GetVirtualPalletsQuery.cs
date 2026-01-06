using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Queries.GetVirtualPallets
{
	public record GetVirtualPalletsQuery(int ProductId, DateOnly BestBefore):IRequest<List<VirtualPallet>>;	
}
