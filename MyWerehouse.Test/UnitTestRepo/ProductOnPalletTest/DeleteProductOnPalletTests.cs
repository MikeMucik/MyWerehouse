using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	public class DeleteProductOnPalletTests : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public DeleteProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		[Fact]
		public void RemoveProduct_DeleteProductFromPallet_RemoveFromPallet()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 10;
			//Act
			_productOnPalletRepo.DeleteProductFromPallet(palletId, productId);
			//Assert
			var result = _context.Pallets
				.FirstOrDefault(p=>p.Id == palletId);					
			Assert.DoesNotContain(result.ProductsOnPallet, p=>p.ProductId == productId);
		}
	}
}
