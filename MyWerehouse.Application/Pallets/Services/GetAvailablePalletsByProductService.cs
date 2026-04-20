using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Services
{
	public class GetAvailablePalletsByProductService(IPalletRepo palletRepo, IProductRepo productRepo) : IGetAvailablePalletsByProductService
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;

		public async Task<List<Pallet>> GetPallets(Guid productId, DateOnly? bestBefore, int amountFullPallet)
		{
			var product = await _productRepo.GetProductByIdAsync(productId);
			var palletsQuery = _palletRepo.GetAvailablePallets(productId, bestBefore)
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
					.FirstOrDefault(),					
				});
			var palletList = await palletsQuery
				.OrderBy(x=>x.BestBefore ?? DateOnly.MaxValue)
				.OrderBy(x=>x.Pallet.Location)
				.ToListAsync();
			var fullPallets = palletList 
				.Where(x => x.Qty == product.CartonsPerPallet)
				.Take(amountFullPallet)
				.Select(x=>x.Pallet)
				.ToList();
			foreach (var pallet in fullPallets)         //do zastanowienia
				pallet.ChangeStatus(PalletStatus.LockedForIssue);
			return fullPallets;			
		}
	}
}
