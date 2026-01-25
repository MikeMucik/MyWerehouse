using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProductToPicking
{
	public class GetAvailablePalletsByProductToPickingHandle:IRequestHandler<GetAvailablePalletsByProductToPickingQuery, List<Pallet>>
	{
		private readonly IPalletRepo _palletRepo;
		public GetAvailablePalletsByProductToPickingHandle(IPalletRepo palletRepo)
		{
			_palletRepo = palletRepo;
		}
		public async Task<List<Pallet>> Handle(GetAvailablePalletsByProductToPickingQuery command, CancellationToken ct)
		{
			var list = await _palletRepo.GetAvailablePallets(command.ProductId, command.BestBefore).ToListAsync(ct);
			return list;
		}
	}
}
