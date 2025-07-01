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
	public class DeleteProductOnPalletTests : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public DeleteProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		//[Fact]
		//public void RemoveProduct_DeleteProductFromPallet_RemoveFromPallet()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q1000",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.ToIssue,
		//		ReceiptId = 10,
		//	};
		//	var product = new ProductOnPallet
		//	{
		//		Id = 1,
		//		PalletId = "Q1000",
		//		ProductId = 10,
		//		Quantity = 100,
		//		DateAdded = DateTime.Now,
		//		BestBefore = new DateOnly(2027, 3, 3)
		//	};
		//	_context.Pallets.Add(pallet1);
		//	_context.ProductOnPallet.Add(product);
		//	_context.SaveChanges();

		//	//Act
		//	var palletId = "Q1000";
		//	var productId = 10;
		//	_productOnPalletRepo.DeleteProductFromPallet(palletId, productId);
		//	//Assert
		//	var result = _context.Pallets
		//		.FirstOrDefault(p => p.Id == palletId);
		//	Assert.DoesNotContain(result.ProductsOnPallet, p => p.ProductId == productId);
		//}
	}
}
