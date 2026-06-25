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

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.RececiptTests.Integration
{
	public class ReceiptCancelDeleteIntegrationTests : TestBase
	{
		private Client CreateClient()
		{
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
			return new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
		}
		private Category CreateCategory(string name)
		{
			return new Category
			{
				Name = name,
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, 1, 56);
		}
		private Location CreateLocation(int id, int position)
		{
			return new Location
			{
				Id = id,
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		[Fact]
		public async Task CancelReceipt_ShouldCancelledReceipt_WhenNotVerifiedReceipt()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test1", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var pallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(location);
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
			var pallets = DbContext.Pallets.Where(p => p.ReceiptId == receipt.Id).ToList();
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
		public async Task DeleteReceipt_ShouldRemoveFromBase_WhenDraftReceipt()
		{
			//Arrange
			var client = CreateClient();
			var location = CreateLocation(1, 1);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			DbContext.Clients.Add(client);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new DeleteDraftReceiptCommand(receipt.Id, "user"));
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Usunięto przyjęcie z bazy", result.Message);
		}
		[Fact]
		public async Task DeleteReceiptAsync_ThrowException_WhenWrongReceiptStatus()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test1", "777777");
			var location = CreateLocation(1, 1);
			
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			//Act&Assert			
			var ex = await Assert.ThrowsAsync<InvalidReceiptStateDomainException>(async () => await Mediator.Send(new DeleteDraftReceiptCommand(receipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains($"Operation prohibited for {receipt.ReceiptNumber} ({receipt.Id}). Incorrect status {receipt.ReceiptStatus}.", ex.Message);

		}
		[Fact]
		public async Task CancelReceipt_ThrowException_WhenVerifiedReceipt()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test1", "777777");
			var location = CreateLocation(1, 1);
			
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Verified, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var pallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(location);
			await DbContext.SaveChangesAsync();
			//Act&Assert		
			var ex = await Assert.ThrowsAsync<ReceiptAlreadyVerifyDomainException>(async () => await Mediator.Send(new CancelReceiptCommand(receipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains($"Receipt {receipt.ReceiptNumber} ({receipt.Id}) already verified. Operation prohibited.", ex.Message);
		}
		[Fact]
		public async Task CancelReceipt_ThrowException_WhenCancelledStatus()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test1", "777777");
			var location = CreateLocation(1, 1);
			
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Cancelled, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var pallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(location);
			await DbContext.SaveChangesAsync();
			//Act&Assert		
			var ex = await Assert.ThrowsAsync<InvalidReceiptStateDomainException>(async () => await Mediator.Send(new CancelReceiptCommand(receipt.Id, "user")));
			Assert.NotNull(ex);
			Assert.Contains($"Operation prohibited for {receipt.ReceiptNumber} ({receipt.Id}). Incorrect status {receipt.ReceiptStatus}.", ex.Message);
		}
	}
}
