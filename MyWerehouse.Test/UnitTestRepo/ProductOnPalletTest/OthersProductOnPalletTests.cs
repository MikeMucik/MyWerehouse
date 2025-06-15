using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	public class OthersProductOnPalletTests : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public OthersProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		[Fact]
		public void CheckIsExists_Exists_ReturnTrue()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			//Act
			var result = _productOnPalletRepo.Exists(palletId, productId);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public async Task CheckIsExists_ExistsAsync_ReturnTrue()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			//Act
			var result = await _productOnPalletRepo.ExistsAsync(palletId, productId);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public void CheckIsExistsBadProductId_Exists_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 888;
			//Act
			var result = _productOnPalletRepo.Exists(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task CheckIsExistsBadProductId_ExistsAsync_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 888;
			//Act
			var result = await _productOnPalletRepo.ExistsAsync(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public void CheckIsExistsBadPalletId_Exists_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1999";
			var productId = 10;
			//Act
			var result = _productOnPalletRepo.Exists(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task CheckIsExistsBadPalletId_ExistsAsync_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1999";
			var productId = 10;
			//Act
			var result = await _productOnPalletRepo.ExistsAsync(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public void RemoveProductsFromPallet_ClearThePallet_ReturnEmpryPallet()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			_productOnPalletRepo.ClearThePallet(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(result);
			Assert.Empty(result.ProductsOnPallet);
		}
		[Fact]
		public async Task RemoveProductsFromPallet_ClearThePalletAsync_ReturnEmpryPallet()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			await _productOnPalletRepo.ClearThePalletAsync(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(result);
			Assert.Empty(result.ProductsOnPallet);
		}
		[Fact]
		public void AddAmount_IncreaseQuantityOnPallet_UpdatedQuantity()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			//Act
			_productOnPalletRepo.IncreaseQuantityOnPallet(palletId, productId, qunatity);
			//Assert
			var pallet = _context.Pallets
				.Include(p => p.ProductsOnPallet)
				//.ThenInclude(pop => pop.Product)
				.FirstOrDefault(p => p.Id == palletId);
			var product = _context.ProductOnPallet
				.Where(p => p.PalletId == palletId)
				.FirstOrDefault(p => p.ProductId == productId);
			Assert.NotNull(pallet);
			//Assert.NotNull(pallet.ProductsOnPallet.Where(p=>p.ProductId ==productId));
			//var quantity = pallet.ProductsOnPallet
			//	.FirstOrDefault(p => p.ProductId == productId)
			//	.Quantity;
			var q1 = product.Quantity;
			Assert.True(0 < q1);
			Assert.Equal(70, q1);
			//Assert.Equal(70, quantity);
		}
		[Fact]
		public async Task AddAmount_IncreaseQuantityOnPalletAsync_UpdatedQuantity()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			//Act
			await _productOnPalletRepo.IncreaseQuantityOnPalletAsync(palletId, productId, qunatity);
			//Assert
			var pallet = _context.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pop => pop.Product)
				.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(pallet);
			var quantity = pallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == productId)
				.Quantity;

			Assert.Equal(70, quantity);
		}
		[Fact]
		public void AddAmount_DecreaseQuantityOnPallet_UpdatedQuantity()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			//Act
			_productOnPalletRepo.DecreaseQuantityOnPallet(palletId, productId, qunatity);
			//Assert
			var pallet = _context.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pop => pop.Product)
				.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(pallet);
			var quantity = pallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == productId)
				.Quantity;
			Assert.Equal(30, quantity);
		}
		[Fact]
		public async Task AddAmount_DecreaseQuantityOnPalletAsync_UpdatedQuantity()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			//Act
			await _productOnPalletRepo.DecreaseQuantityOnPalletAsync(palletId, productId, qunatity);
			//Assert
			var pallet = _context.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pop => pop.Product)
				.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(pallet);
			var quantity = pallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == productId)
				.Quantity;
			Assert.Equal(30, quantity);
		}
	}
}
