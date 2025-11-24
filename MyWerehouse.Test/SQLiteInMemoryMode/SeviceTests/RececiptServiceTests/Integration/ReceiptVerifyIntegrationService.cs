using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptVerifyIntegrationService : ReceiptIntegratioCommandService
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
				Name = "Category A",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Product A",
				SKU = "123456",
				CategoryId = 1,
				IsDeleted = false
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = new Pallet
			{
				Id = "PAL001",
				Location = location,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
						{				
				Product = product,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var receipt = new Receipt
			{				
				Client = client,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U001",
				Pallets = [pallet]
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await _receiptService.VerifyAndFinalizeReceiptAsync(receipt.Id, "U001");

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Contains("Palety z przyjęcia zweryfikowano, gotowe do działania", result.Message);

			var receiptVeryfying = await DbContext.Receipts.FindAsync(receipt.Id);
			Assert.NotNull(receiptVeryfying);
			receiptVeryfying.ReceiptStatus.Should().Be(ReceiptStatus.Verified);

			var updatedPallet = await DbContext.Pallets.FindAsync(pallet.Id);
			Assert.NotNull(updatedPallet);
			updatedPallet.Status.Should().Be(PalletStatus.InStock);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.Id == receipt.Id);
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
				Name = "Category A",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Product A",
				SKU = "123456",
				CategoryId = 1,
				IsDeleted = false
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = new Pallet
			{
				Id = "PAL001",
				Location = location,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
						{				
				Product = product,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var receipt = new Receipt
			{				
				Client = client,
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "U001",
				Pallets = [pallet]
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await _receiptService.VerifyAndFinalizeReceiptAsync(receipt.Id, "U001");

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains("Nie można zweryfikować przyjęcia", result.Message);
		}
	}
}
