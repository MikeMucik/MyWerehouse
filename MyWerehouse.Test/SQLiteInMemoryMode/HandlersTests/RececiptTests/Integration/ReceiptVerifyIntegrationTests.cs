using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Receviving.ReceivingExceptions;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.RececiptTests.Integration
{
	public class ReceiptVerifyIntegrationTests : TestBase
	{
		Guid productId = Guid.NewGuid();
		Guid productId1 = Guid.NewGuid();
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
		private Product CreateProduct(Guid id, string name, string sku)
		{
			return Product.CreateForSeed(id, name, sku, DateTime.UtcNow.AddMonths(-5), 1, false, 56);
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
		private Inventory CreateInventory(Guid id, int quantity)
		{
			return new Inventory
			{
				ProductId = id,
				Quantity = quantity,
				LastUpdated = DateTime.UtcNow.AddDays(-1)
			};
		}
		[Fact]
		public async Task VerifyAndFinalizeReceipt_UpdatesStatusAndInventory_WhenOneProduct()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct(productId, "Product A", "123456");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2027, 3, 3));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await Mediator.Send(new VerifyAndFinalizeReceiptCommand(receipt.Id, "U001"));
			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("Palety z przyjęcia zweryfikowano, gotowe do użycia.", result.Message);
			var receiptVeryfying = await DbContext.Receipts.FindAsync(receipt.Id);
			Assert.NotNull(receiptVeryfying);
			receiptVeryfying.ReceiptStatus.Should().Be(ReceiptStatus.Verified);
			var updatedPallet = await DbContext.Pallets.FindAsync(pallet.Id);
			Assert.NotNull(updatedPallet);
			updatedPallet.Status.Should().Be(PalletStatus.InStock);
			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == receipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Verified, historyRecipt.StatusAfter);
			var inventory = await DbContext.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.Id);
			inventory.Should().NotBeNull();
			inventory.Quantity.Should().Be(10);
		}
		[Fact]
		public async Task VerifyAndFinalizeReceipt_UpdatesStatusAndInventory_WhenTwoProduct()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct(productId, "Product A", "123456");
			var product1 = CreateProduct(productId1, "Product B", "234567");
			var location = CreateLocation(1, 1);
			var inventory = CreateInventory(productId, 100);
			var inventory1 = CreateInventory(productId1, 100);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2027, 3, 3));
			var pallet1 = Pallet.CreateForTests("Q1001", DateTime.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2027, 3, 3));
			var pallet2 = Pallet.CreateForTests("Q1002", DateTime.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
			pallet2.AddProduct(product1.Id, 10, new DateOnly(2027, 3, 3));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet, pallet1, pallet2);
			DbContext.Receipts.Add(receipt);
			DbContext.Inventories.AddRange(inventory, inventory1);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await Mediator.Send(new VerifyAndFinalizeReceiptCommand(receipt.Id, "U001"));
			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("Palety z przyjęcia zweryfikowano, gotowe do użycia.", result.Message);
			var receiptVeryfying = await DbContext.Receipts.FindAsync(receipt.Id);
			Assert.NotNull(receiptVeryfying);
			receiptVeryfying.ReceiptStatus.Should().Be(ReceiptStatus.Verified);
			var updatedPallet = await DbContext.Pallets.FindAsync(pallet.Id);
			Assert.NotNull(updatedPallet);
			Assert.All(
			DbContext.Pallets.Where(p => p.ReceiptId == receipt.Id),
			p => Assert.Equal(PalletStatus.InStock, p.Status));
			//updatedPallet.Status.Should().Be(PalletStatus.InStock);
			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == receipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Verified, historyRecipt.StatusAfter);
			var inventoryAfter = DbContext.Inventories.Single(p => p.ProductId == product.Id);
			var inventoryAfter1 = DbContext.Inventories.Single(p => p.ProductId == product1.Id);
			Assert.Equal(120, inventoryAfter.Quantity); //100+Q1000+Q1001 = 120
			Assert.Equal(110, inventoryAfter1.Quantity); //100+Q1002 = 110
			Assert.Equal(2, await DbContext.Inventories.CountAsync());
		}
		[Fact]
		public async Task VerifyAndFinalizeReceipt_ShouldReturnError_WhenReceiptAlreadyVerified()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct(productId, "Product A", "123456");
			var location = CreateLocation(1, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Receiving, null, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2027, 3, 3));
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.Verified, 1);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act&Assert		
			var ex = await Assert.ThrowsAsync<ReceiptAlreadyVerifyDomainException>(() => Mediator.Send(new VerifyAndFinalizeReceiptCommand(receipt.Id, "U001")));
			Assert.Equal($"Receipt {receipt.ReceiptNumber} ({receipt.Id}) already verified. Operation prohibited.", ex.Message);

		}
	}
}
