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
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.ReceiptTests.Integration
{
	public class ReceiptCompletePhysicalIntegrationTests : TestBase
	{
		private static Client CreateClient()
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
		private static Category CreateCategory(string name)
		{
			return new Category
			{
				Name = name,
				IsDeleted = false
			};
		}
		private static Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, TestDates.UtcNow, 1, 56, 30, 30, 30, 30, "TestDetails");
		}
		private static Location CreateLocation(int id, int position)
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
		public async Task VerifyAndFinalizeReceipt_UpdatesStatus_WhenValidData()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Product A", "123456");
			var location = CreateLocation(1, 1);			
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.InProgress, 1);

			var pallet = Pallet.CreateForTests("PAL001", TestDates.UtcNow, 1, PalletStatus.Receiving, receipt.Id, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, new DateOnly(2026, 1, 1));
			var pallet1 = Pallet.CreateForTests("PAL002", TestDates.UtcNow, 1, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, new DateOnly(2026, 1, 1));
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
			Assert.True(result.IsSuccess);
			Assert.Equal(ReceiptStatus.PhysicallyCompleted, receipt.ReceiptStatus);
		}
		[Fact]
		public async Task VerifyAndFinalizeReceipt_ShouldDontUpdatesStatus_WhenReceiptStatusNew()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Product A", "123456");
			var location = CreateLocation(1, 1);			
			var pallet = Pallet.CreateForTests("PAL001", TestDates.UtcNow, 1, PalletStatus.Receiving, null, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, new DateOnly(2026, 1, 1));
			var pallet1 = Pallet.CreateForTests("PAL002", TestDates.UtcNow, 1, PalletStatus.Receiving, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, new DateOnly(2026, 1, 1));
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);			
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act&Assert		
			var ex = await Assert.ThrowsAsync<InvalidReceiptStateDomainException>(() => Mediator.Send(new CompletePhysicalReceiptCommand(receipt.Id, "user")));
			Assert.Equal($"Operation prohibited for {receipt.ReceiptNumber} ({receipt.Id}). Incorrect status {receipt.ReceiptStatus}.", ex.Message);
		}
	}
}