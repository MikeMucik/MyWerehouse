using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public void AddProductToPallet(ProductOnPallet product)
		{
			_werehouseDbContext.ProductOnPallet.Add(product);
			_werehouseDbContext.SaveChanges();
		}

		public void DeleteProductFromPallet(string palletId, int productId)
		{
			var productOnPallet = _werehouseDbContext.ProductOnPallet
				.FirstOrDefault(p=>p.ProductId == productId &&p.PalletId ==palletId);
			if (productOnPallet != null)
			{
				_werehouseDbContext.ProductOnPallet.Remove(productOnPallet);
				_werehouseDbContext.SaveChanges();
			}
		}
		public void UpdateProductQuantity(string palletId, int productId, int newQuantity)
		{
			var productOnPallet = _werehouseDbContext.ProductOnPallet
				.FirstOrDefault(p => p.PalletId == palletId && p.ProductId == productId);
			if (productOnPallet == null)
			{
				throw new InvalidOperationException("Produkt nie istnieje na palecie");
			}
			productOnPallet.Quantity = newQuantity;
			productOnPallet.DateAdded = DateTime.Now;
			_werehouseDbContext.SaveChanges();
		}
		public IQueryable<ProductOnPallet> GetProductsOnPallets(string palletId)
		{
			return _werehouseDbContext.ProductOnPallet
				.OrderBy(p=>p.Product.Name).Where(p=>p.PalletId==palletId);
		}
		public bool Exists(string palletId, int productId)
		{
			return  _werehouseDbContext.ProductOnPallet
				.Any(p => p.PalletId == palletId && p.ProductId == productId);
			
		}
		public void ClearThePallet(string palletId)
		{
			var productsOnPallet = _werehouseDbContext.ProductOnPallet
				.Where(p => p.PalletId == palletId)	;
			if (productsOnPallet.Any())
			{
				_werehouseDbContext.ProductOnPallet.RemoveRange(productsOnPallet);
				_werehouseDbContext.SaveChanges();
			}			
		}
		public int GetQuantity(string palletId, int productId)
		{
			var item =  _werehouseDbContext.ProductOnPallet
				.FirstOrDefault(p=>p.PalletId == palletId&& p.ProductId == productId);
			if (item == null)
			{
				throw new InvalidOperationException("Nie ma produktu na palecie");
			}
			return item.Quantity;
		}

	}
}
