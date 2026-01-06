using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct
{
	public record GetAvailablePalletsByProductQuery(int ProductId, DateOnly? BestBefore, int Reserved, int NeededCartoons) : IRequest<List<Pallet>>;	
}
