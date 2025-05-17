using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;
using Xunit.Sdk;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	public class UpdateProductOnPalletTests : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public UpdateProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		[Fact]
		public void ChangeQuantity_UpdateProductQuantity_ReturnUpdatedQuantity()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			var newQuantity = 25;
			//Act
			_productOnPalletRepo.UpdateProductQuantity(palletId, productId, newQuantity);	
			//Assert
			var result = _context.ProductOnPallet
				.FirstOrDefault(p=>p.PalletId == palletId && p.ProductId ==  productId);
			Assert.NotNull(result);
			Assert.Equal(newQuantity, result.Quantity);
		}
		[Fact]
		public void ChangeQuantityNotExistProduct_UpdateProductQuantity_ThrowException()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 1012;
			var newQuantity = 25;
			//Act&Assert
			var ex = Assert.Throws<InvalidOperationException>(() => 
			_productOnPalletRepo.UpdateProductQuantity(palletId, productId, newQuantity));
			Assert.Equal("Produkt nie istnieje na palecie", ex.Message);
		}
	}
}
