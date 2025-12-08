using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.PickingPallets.Qeuries.GetVirtualPallets
{
	public class GetVirtualPalletsHandler : IRequestHandler<GetVirtualPalletsQuery, List<VirtualPallet>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		public GetVirtualPalletsHandler(IPickingPalletRepo pickingPalletRepo)
		{
			_pickingPalletRepo = pickingPalletRepo;
		}
		public async Task<List<VirtualPallet>> Handle(GetVirtualPalletsQuery query, CancellationToken ct)
		{
			var list = await _pickingPalletRepo.GetVirtualPalletsByBBAsync(query.ProductId, query.BestBefore)
				?? throw new PalletException("Brak palety do pickingu - błąd virtual");
			return list;
		}

	}
}
