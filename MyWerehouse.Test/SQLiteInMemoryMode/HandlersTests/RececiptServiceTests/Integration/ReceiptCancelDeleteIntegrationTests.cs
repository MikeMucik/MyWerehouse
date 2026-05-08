using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Receipts.Commands.CancelReceipt;
using MyWerehouse.Application.Receipts.Commands.DeleteDraftReceipt;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Receviving.ReceivingExceptions;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptCancelDeleteIntegrationTests : TestBase
	{		
		[Fact]
		public async Task DeleteReceiptAsync_NotVerifiedReceipt_CancelledInBase()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var product = Product.Create("Test", "666666", 1, 56);
		
			var product1 = Product.Create("Test", "666666", 1, 56);

			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			initialPallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var initialPallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			initialPallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(product, product1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new CancelReceiptCommand(receipt.Id, "user"));
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Anulowano przyjęcie wraz z paletami z bazy", result.Message);
			var receiptE = DbContext.Receipts.FirstOrDefault(r => r.Id == receipt.Id);
			Assert.NotNull(receiptE);
			Assert.Equal(ReceiptStatus.Cancelled, receiptE.ReceiptStatus);
			Assert.NotNull(DbContext.Pallets);
			var pallets = DbContext.Pallets.Where(p=>p.ReceiptId == receipt.Id).ToList();
			foreach (var palet in pallets)
			{
				Assert.Equal(PalletStatus.Cancelled, palet.Status);
			}
			Assert.NotNull(DbContext.ProductOnPallet);

			Assert.Equal(2, DbContext.Products.Count());
			Assert.Equal(1, DbContext.Categories.Count());
			Assert.Equal(1, DbContext.Clients.Count());
		}
		[Fact]
		public async Task DeleteReceiptAsync_NotVerifiedReceipt_RemovedBase()
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
				Addresses = [address]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};

			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
				DbContext.Clients.Add(client);
			DbContext.Receipts.Add(initialReceipt);		
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new DeleteDraftReceiptCommand(initialReceipt.Id, "user"));
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Usunięto przyjęcie z bazy", result.Message);
		}
		[Fact]
		public async Task DeleteReceiptAsync_WrongReceiptStatus_ThrowExceptione()
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
				Addresses = [address]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};

			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			//Act&Assert			
			var ex = await Assert.ThrowsAsync<InvalidReceiptStateException>(async () => await Mediator.Send(new DeleteDraftReceiptCommand(receipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains($"Operation prohibited for {receipt.Id}. Incorrect status {receipt.ReceiptStatus}.", ex.Message);

		}
		[Fact]
		public async Task CancelReceiptAsync_VerifiedReceipt_ThrowInfo()
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialCategory = new Category
			{	Id =1,
				Name = "name",
				IsDeleted = false
			};
			var initialLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = Product.Create("Test", "666666", 1, 56);
			
			var initialProduct1 = Product.Create("Test", "666666", 1, 56);
			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Verified, 1);
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, initialReceipt.Id, null);
			initialPallet.AddProduct(initialProduct.Id, 100, new DateOnly(2027, 3, 3));
			var initialPallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, initialReceipt.Id, null);
			initialPallet1.AddProduct(initialProduct1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert		
			var ex = await Assert.ThrowsAsync<ReceiptAlreadyVerifyException>(async()=> await Mediator.Send(new CancelReceiptCommand(initialReceipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains($"Receipt {initialReceipt.Id} already verified. Operation prohibited.", ex.Message);		
		}
		[Fact]
		public async Task CancelReceiptAsync_WrongStatus_ThrowInfo()
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var initialLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = Product.Create("Test", "666666", 1, 56);

			var initialProduct1 = Product.Create("Test", "666666", 1, 56);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Cancelled, 1);
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			initialPallet.AddProduct(initialProduct.Id, 100, new DateOnly(2027, 3, 3));
			var initialPallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			initialPallet1.AddProduct(initialProduct1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert		
			var ex = await Assert.ThrowsAsync<InvalidReceiptStateException>(async () => await Mediator.Send(new CancelReceiptCommand(receipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains($"Operation prohibited for {receipt.Id}. Incorrect status {receipt.ReceiptStatus}.", ex.Message);
		}
	}
}
