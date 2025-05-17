using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	public class OthersProductOnPalletTests :CommandTestBase
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
			Assert. True(result);
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
		public void RemoveProductsFromPallet_ClearThePallet_ReturnEmpryPallet()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			_productOnPalletRepo.ClearThePallet(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p=>p.Id == palletId);
			Assert.NotNull(result);
			Assert.Empty(result.ProductsOnPallet);
		}
	}
}
