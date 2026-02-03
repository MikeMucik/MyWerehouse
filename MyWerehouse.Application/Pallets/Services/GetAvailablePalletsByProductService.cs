using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Services
{
	public class GetAvailablePalletsByProductService : IGetAvailablePalletsByProductService
	{
		private readonly IPalletRepo _repo;
		private readonly IProductRepo _productRepo;
		public GetAvailablePalletsByProductService(IPalletRepo palletRepo, IProductRepo productRepo)
		{
			_repo = palletRepo;
			_productRepo = productRepo;
		}
		public async Task<List<Pallet>> GetPallets(int productId, DateOnly? bestBefore, int amountFullPallet)
		{
			var product = await _productRepo.GetProductByIdAsync(productId)??
				throw new NotFoundProductException(productId);
			var palletsQuery = _repo.GetAvailablePallets(productId, bestBefore)
				.Select(p => new
				{
					Pallet = p,
					Qty = p.ProductsOnPallet
					.Where(x => x.ProductId == productId)
					.Select(x => x.Quantity)
					.FirstOrDefault(),
					BestBefore = p.ProductsOnPallet
					.Where(x => x.ProductId == productId)
					.Select(x => x.BestBefore)
					.FirstOrDefault()
				});
				//.OrderByDescending(x => x.BestBefore);
			var palletList = await palletsQuery
				.OrderBy(x=>x.BestBefore)
				.ToListAsync();
			var fullPallets = palletList
				.Where(x => x.Qty == product.CartonsPerPallet)
				.Take(amountFullPallet)
				.Select(x=>x.Pallet)
				.ToList();
			foreach (var pallet in fullPallets)
				pallet.Status = PalletStatus.InTransit;
			return fullPallets;
			//if (paletList.Count == 0) return new List<Pallet>();
			//int taken = amountFullPallet;
			//int sum = paletList.Take(taken).Sum(x => x.Qty);
			//while (sum < neededCartoons && taken < paletList.Count)
			//{
			//	taken++;
			//	sum = paletList.Take(taken).Sum(x => x.Qty);
			//}
			//throw new NotImplementedException();
		}
	}
}
