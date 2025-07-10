using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	public class DeleteProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public void HideProduct_DeleteProduct_ChangeNotActive()
		{
			//Arrange
			var product1 = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var pallet = new Pallet
			{
				Id = "Q1234",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var product = new ProductOnPallet
			{
				PalletId = "Q1234",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var receipt =new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = new List<Pallet> { pallet},
				PerformedBy = "U005"
			};
			
			_context.ProductOnPallet.Add(product);
			_context.Receipts.Add(receipt);
			_context.Products.Add(product1);
			_context.SaveChanges();
			var productId = 10;
			//Act
			_productService.DeleteProduct(productId);
			//Assert
			var resultProduct = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.NotNull(resultProduct);
			Assert.True(resultProduct.IsDeleted);
		}
		[Fact]
		public void RemoveProduct_DeleteProduct_DeleteFromList()
		{
			//Arrange
			var product1 = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			_context.Products.Add(product1);
			_context.SaveChanges();
			var productId = 10;
			//Act
			_productService.DeleteProduct(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.Null(product);
		}
		[Fact]
		public void NotProperIdProduct_DeleteProduct_ThrowException()
		{
			//Arrange
			var productId = 9891;
			//Act&Assert
			var e = Assert.Throws<InvalidDataException>(() => _productService.DeleteProduct(productId));
			Assert.NotNull(e);
			Assert.Contains("Brak produktu o tym numerze", e.Message);
		}
		[Fact]
		public async Task HideProduct_DeleteProductAsync_ChangeNotActive()
		{
			//Arrange
			var product1 = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var pallet = new Pallet
			{
				Id = "Q1234",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var product = new ProductOnPallet
			{
				PalletId = "Q1234",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var receipt = new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = new List<Pallet> { pallet },
				PerformedBy = "U005"
			};

			_context.ProductOnPallet.Add(product);
			_context.Receipts.Add(receipt);
			_context.Products.Add(product1);
			_context.SaveChanges();
			var productId = 10;
			//Act
			await _productService.DeleteProductAsync(productId);
			//Assert
			var result = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
		[Fact]
		public async Task Product_DeleteProductAsync_DeleteFromList()
		{
			//Arrange
			var product1 = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			_context.Products.Add(product1);
			_context.SaveChanges();
			var productId = 10;
			//Act
			await _productService.DeleteProductAsync(productId);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == productId);
			Assert.Null(product);
		}
		[Fact]
		public async Task NotProperIdProduct_DeleteProductAsync_ThrowException()
		{
			//Arrange
			var productId = 9891;
			//Act&Assert
			var e =await Assert.ThrowsAsync<InvalidDataException>(() => _productService.DeleteProductAsync(productId));
			Assert.NotNull(e);
			Assert.Contains("Brak produktu o tym numerze", e.Message);
		}
	}
}
