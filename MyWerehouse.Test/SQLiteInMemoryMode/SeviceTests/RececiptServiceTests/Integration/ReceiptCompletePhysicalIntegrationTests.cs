using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptCompletePhysicalIntegrationTests : TestBase
	{
		[Fact]
		public async Task VerifyAndFinalizeReceiptAsync_WhenValid_UpdatesStatusAndInventory()
		{
			// Arrange
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
				Addresses = [address]
			};
			var category = new Category
			{
				Id = 1,
				Name = "Category A",
				IsDeleted = false
			};
			var product = Product.Create("Product A", "123456", 1, 56);
			
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = Pallet.CreateForTests("PAL001", DateTime.UtcNow, 1, PalletStatus.New, null, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			//var pallet = new Pallet
			//{
			//	PalletNumber = "PAL001",
			//	Location = location,
			//	Status = PalletStatus.Receiving,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{new ProductOnPallet
			//			{
			//	Product = product,
			//	Quantity = 10,
			//	DateAdded = DateTime.UtcNow
			//			}
			//	}
			//};
			var pallet1 = Pallet.CreateForTests("PAL002", DateTime.UtcNow, 1, PalletStatus.New, null, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			//var pallet1 = new Pallet
			//{
			//	PalletNumber = "PAL002",
			//	Location = location,
			//	Status = PalletStatus.Receiving,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{new ProductOnPallet            {
			//	Product = product,
			//	Quantity = 10,
			//	DateAdded = DateTime.UtcNow
			//			}
			//	}
			//};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.InProgress, 1);
			
			//var receipt = new Receipt
			//{
			//	Id = receiptId1,
			//	ReceiptNumber = 2,
			//	Client = client,
			//	ReceiptStatus = ReceiptStatus.InProgress,
			//	PerformedBy = "U001",
			//	Pallets = [pallet, pallet1]
			//};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Receipts.Add(receipt);
			receipt.AttachPallet(pallet, location, "U001");
			receipt.AttachPallet(pallet1, location, "U001");
			await DbContext.SaveChangesAsync();
			// Act
			var result = await Mediator.Send(new CompletePhysicalReceiptCommand(receipt.Id, "user"));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal(ReceiptStatus.PhysicallyCompleted, receipt.ReceiptStatus);
		}
		[Fact]
		public async Task VerifyAndFinalizeReceiptAsync_WhenInValid_DontUpdatesStatusAndInventory()
		{
			// Arrange
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
				Addresses = [address]
			};
			var category = new Category
			{Id = 1,
				Name = "Category A",
				IsDeleted = false
			};
			var product = Product.Create("Product A", "123456", 1, 56);
			
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = Pallet.CreateForTests("PAL001", DateTime.UtcNow, 1, PalletStatus.Receiving, null, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			//var pallet = new Pallet
			//{
			//	PalletNumber = "PAL001",
			//	Location = location,
			//	Status = PalletStatus.Receiving,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{new ProductOnPallet
			//			{
			//	Product = product,
			//	Quantity = 10,
			//	DateAdded = DateTime.UtcNow
			//			}
			//	}
			//};
			var pallet1 = Pallet.CreateForTests("PAL002", DateTime.UtcNow, 1, PalletStatus.Receiving, null, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			//var pallet1 = new Pallet
			//{
			//	PalletNumber = "PAL002",
			//	Location = location,
			//	Status = PalletStatus.Receiving,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{new ProductOnPallet            {
			//	Product = product,
			//	Quantity = 10,
			//	DateAdded = DateTime.UtcNow
			//			}
			//	}
			//};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			//receipt.AttachPallet(pallet, location, "U001");
			//receipt.AttachPallet(pallet1, location, "U001");
			//var receipt = new Receipt
			//{
			//	Id = receiptId1,
			//	ReceiptNumber = 1,
			//	Client = client,
			//	ReceiptStatus = ReceiptStatus.Planned,
			//	PerformedBy = "U001",
			//	Pallets = [pallet, pallet1]
			//};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await Mediator.Send(new CompletePhysicalReceiptCommand(receipt.Id, "user"));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
		}
	}
}