using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetOneAvailablePalletByProduct
{
	public record GetOneAvailablePalletByProductQuery(int ProductId, DateOnly? BestBefore): IRequest<Pallet>;
	
}
