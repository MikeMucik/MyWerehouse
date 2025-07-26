using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	[Collection("QuerryCollection")]
	public class ViewProductOnPallet : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public ViewProductOnPallet(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		[Fact]
		public void ShowProductsOnPallet_GetProductOnPallets_ReturnListOfProducts()
		{
			//Arrange
			var palletId = "Q1001";
			//Act
			var result = _productOnPalletRepo.GetProductsOnPallets(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			//Assert.All(result, item =>
			//{
			//	Assert.Contains(item.ProductId, new[] { 10, 11 });
			//	Assert.Contains(item.Quantity, new[] { 100, 200 });
			//});
			var expected = new List<(int ProductId, int Quantity)>
			{
				(10,100),
				(11, 200),
			};
			foreach (var item in result)
			{
				Assert.Contains((item.ProductId, item.Quantity), expected);
			}
		}		
		[Fact]
		public async Task ShowQuantityOfProductOnPallet_GetQuantityAsync_ReturnData()
		{
			//Arrange
			var palletId = "Q1001";
			var productId = 10;
			//Act
			var result = await _productOnPalletRepo.GetQuantityAsync(palletId, productId);
			//Assert
			Assert.NotEqual(0,result);
			Assert.Equal(100, result);
		}		
		[Fact]
		public async Task ShowQuantityOfProductNotExistOnPallet_GetQuantityAsync_ReturnException()
		{
			//Arrange
			var palletId = "Q1001";
			var productId = 1023;
			//Act&Assert
			var ex =await Assert.ThrowsAsync<InvalidOperationException>(async () => await _productOnPalletRepo.GetQuantityAsync(palletId, productId));
			Assert.Equal("Nie ma produktu na palecie", ex.Message);
		}		
	}
}
