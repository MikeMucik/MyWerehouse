using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProductToPicking
{
	public record GetAvailablePalletsByProductToPickingQuery(int ProductId, DateOnly? BestBefore):IRequest<List<Pallet>>;
	
}
