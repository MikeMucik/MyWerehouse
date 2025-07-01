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
	public class AddProductOnPalletTests: CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public AddProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		//[Fact]
		//public void AddProduct_AddProductToPallet_PalletConatinsProduct()
		//{
		//	//Arrange
		//	var product = new ProductOnPallet
		//	{
		//		PalletId = "Q1234",
		//		ProductId = 10,
		//		Quantity = 100,
		//		DateAdded = DateTime.Now,
		//		BestBefore = new DateOnly(2027,3,3)
		//	};
		//	//Act
		//	_productOnPalletRepo.AddProductToPallet(product);
		//	//Assert
		//	var result = _context.ProductOnPallet.FirstOrDefault(p=>p.PalletId == product.PalletId);
		//	Assert.NotNull(result);
		//	Assert.Equal(product, result);
		//	Assert.Equal(product.Quantity, result.Quantity);
		//}
	}
}
