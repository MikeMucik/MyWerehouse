using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetOneAvailablePalletByProduct
{
	public class GetOneAvailablePalletByProductHandler: IRequestHandler<GetOneAvailablePalletByProductQuery, Pallet>
	{
		private readonly IPalletRepo _palletRepo;
		public GetOneAvailablePalletByProductHandler(IPalletRepo palletRepo)
		{
			_palletRepo = palletRepo;
		}
		public async Task<Pallet> Handle (GetOneAvailablePalletByProductQuery request, CancellationToken ct)
		{
			var pallets = _palletRepo.GetAvailablePallets(request.ProductId, request.BestBefore);
			var pallet = await pallets.FirstOrDefaultAsync() ?? throw new PalletNotFoundException("Brak palet do pickingu");			
			return pallet;				
		}
	}
}
