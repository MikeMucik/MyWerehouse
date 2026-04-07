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
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptVerifyIntegrationTests : TestBase
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
				Id =1,
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
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			var pallet = Pallet.CreateForTests("PAL001", DateTime.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
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
			Assert.Contains("Palety z przyjęcia zweryfikowano, gotowe do działania", result.Message);

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
		public async Task VerifyAndFinalizeReceiptAsync_WhenInValid_NoUpdatesStatusAndInventory()
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
				Id =1,
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
			
			// Act
			var result = await Mediator.Send(new VerifyAndFinalizeReceiptCommand(receipt.Id, "U001"));

			// Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Nie można zweryfikować przyjęcia", result.Error);
		}
	}
}
