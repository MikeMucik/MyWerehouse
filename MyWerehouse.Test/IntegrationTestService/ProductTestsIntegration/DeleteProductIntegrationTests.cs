using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	public class DeleteProductIntegrationTests : ProductIntegrationCommand
	{
		[Fact]
		public async Task HideProduct_DeleteProductAsync_ChangeNotActive()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var product1 = Product.Create("Test", "666666", 1, 56);
			_context.Clients.Add(client);
			_context.Products.Add(product1);
			_context.Locations.Add(location);
			_context.SaveChanges();
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var pallet = Pallet.CreateForTests("Q1234", DateTime.Now, location.Id, PalletStatus.Available, null, null);
			pallet.AddProduct(product1.Id, 100, new DateOnly(2027, 3, 3));		
			
			var receipt = Receipt.CreateForSeed(receiptId1, 1, client.Id, "U005",
			DateTime.UtcNow, ReceiptStatus.Verified, location.Id);	
			receipt.AttachPallet(pallet, location, "U0005");
			_context.Receipts.Add(receipt);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();			
			var productId = product1.Id;
			//Act
			await _productService.DeleteProductAsync(product1.Id);
			//Assert
			var result = _context.Products.FirstOrDefault(p => p.Id == product1.Id);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
		//var pallet = new Pallet
		//{
		//	PalletNumber = "Q1234",
		//	DateReceived = DateTime.Now,
		//	LocationId = 1,
		//	Status = PalletStatus.Available,
		//	//ReceiptId = 10,
		//};
		//var product = new ProductOnPallet
		//{
		//	//PalletId = "Q1234",
		//	Pallet = pallet,
		//	//ProductId = 10,
		//	Product = product1,
		//	Quantity = 100,
		//	DateAdded = DateTime.Now,
		//	BestBefore = new DateOnly(2027, 3, 3)
		//};
		//var receipt = new Receipt
		//{
		//	Id = receiptId1,
		//	ReceiptDateTime = DateTime.Now,
		//	ClientId = 1,
		//	Pallets = new List<Pallet> { pallet },
		//	PerformedBy = "U005"

		//};

		//_context.ProductOnPallet.Add(product);
		[Fact]
		public async Task Product_DeleteProductAsync_DeleteFromList()
		{
			//Arrange
			var product1 = Product.Create("Test", "666666", 1, 56);
			
			_context.Products.Add(product1);
			_context.SaveChanges();
			
			//Act
			await _productService.DeleteProductAsync(product1.Id);
			//Assert
			var product = _context.Products.FirstOrDefault(p => p.Id == product1.Id);
			Assert.Null(product);
		}
		[Fact]
		public async Task NotProperIdProduct_DeleteProductAsync_ThrowException()
		{
			//Arrange
			var productId =Guid.Parse("00000000-0000-0000-0000-000000000000");
			//Act&Assert
			var result =await _productService.DeleteProductAsync(productId);
			//Assert.NotNull(e);
			Assert.NotNull(result);
			Assert.Contains("Brak produktu o tym numerze", result.Error);
		}
	}
}
