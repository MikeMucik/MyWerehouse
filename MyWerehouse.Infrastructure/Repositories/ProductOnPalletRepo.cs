using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class ProductOnPalletRepo : IProductOnPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public ProductOnPalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
				
		public async Task AddProductToPalletAsync(ProductOnPallet product)
		{
			await _werehouseDbContext.ProductOnPallet.AddAsync(product);
			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task DeleteProductFromPalletAsync(string palletId, int productId)
		{
			var productOnPallet = await _werehouseDbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.ProductId == productId && p.PalletId == palletId);
			if (productOnPallet != null)
			{
				_werehouseDbContext.ProductOnPallet.Remove(productOnPallet);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}		
		public async Task UpdateProductQuantityAsync(string palletId, int productId, int newQuantity)
		{
			var productOnPallet = await _werehouseDbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == palletId && p.ProductId == productId)
				?? throw new InvalidDataException("Produkt nie istnieje na palecie");
			productOnPallet.Quantity = newQuantity;
			productOnPallet.DateAdded = DateTime.Now;
			await _werehouseDbContext.SaveChangesAsync();
		}	
		public IQueryable<ProductOnPallet> GetProductsOnPallets(string palletId)
		{
			return _werehouseDbContext.ProductOnPallet
				.OrderBy(p => p.Product.Name).Where(p => p.PalletId == palletId);
		}		
		public async Task<bool> ExistsAsync(string palletId, int productId)
		{
			return await _werehouseDbContext.ProductOnPallet
				.AnyAsync(p => p.PalletId == palletId && p.ProductId == productId);
		}		
		public async Task ClearThePalletAsync(string palletId)
		{
			var productsOnPallet = _werehouseDbContext.ProductOnPallet
				.Where(p => p.PalletId == palletId);
			if (await productsOnPallet.AnyAsync())
			{
				_werehouseDbContext.ProductOnPallet.RemoveRange(productsOnPallet);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}		
		public async Task<int> GetQuantityAsync(string palletId, int productId)
		{
			var item = await _werehouseDbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == palletId && p.ProductId == productId);
			if (item == null)
			{
				throw new InvalidOperationException("Nie ma produktu na palecie");
			}
			return item.Quantity;
		}		
		public async Task IncreaseQuantityOnPalletAsync(string palletId, int productId, int quantity)
		{
			var productOnPallet = await _werehouseDbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == palletId && p.ProductId == productId);			
				productOnPallet.Quantity += quantity;			
			productOnPallet.DateAdded = DateTime.Now;
			await _werehouseDbContext.SaveChangesAsync();
		}		
		public async Task DecreaseQuantityOnPalletAsync(string palletId, int productId, int quantity)
		{
			var productOnPallet =await _werehouseDbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == palletId && p.ProductId == productId);
			productOnPallet.Quantity -= quantity;			
			productOnPallet.DateAdded = DateTime.Now;
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<List<QuantityLocation>> GetQuantityLocation(int productId)
		{
			var list = await _werehouseDbContext.ProductOnPallet
				.Where(pop => pop.ProductId == productId)
				.Include(pop => pop.Pallet)
				.Select(pop => new
				{
					LocationId = pop.Pallet.LocationId,
					Quantity = pop.Quantity
				})
				.GroupBy(p => p.LocationId)
				.Select(g => new QuantityLocation { LocationId = g.Key, Quantity = g.Sum(x => x.Quantity) })
				.ToListAsync();
			return list;
		}
	}
}
