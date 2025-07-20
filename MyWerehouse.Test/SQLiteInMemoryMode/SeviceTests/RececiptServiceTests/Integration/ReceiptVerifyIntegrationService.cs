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
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptVerifyIntegrationService : ReceiptIntegratioCommandService
	{
		[Fact]
		public async Task VerifyAndFinalizeReceiptAsync_UpdatesStatusAndInventory_WhenValid()
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
				Id = 1,
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
			var product = new Product
			{
				Id = 1,
				Name = "Product A",
				SKU = "123456",
				CategoryId = 1,
				IsDeleted = false
			};
			var location = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet = new Pallet
			{
				Id = "PAL001",
				LocationId = 1,
				Status = PalletStatus.Receiving,
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
						{
				Id = 1,
				ProductId = 1,
				Quantity = 10,
				DateAdded = DateTime.UtcNow
						}
				}
			};
			var receipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
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
			await _receiptService.VerifyAndFinalizeReceiptAsync(receipt.Id, "U001");

			// Assert
			var updatedReceipt = await DbContext.Receipts.FindAsync(receipt.Id);
			updatedReceipt.ReceiptStatus.Should().Be(ReceiptStatus.Verified);

			var updatedPallet = await DbContext.Pallets.FindAsync(pallet.Id);
			updatedPallet.Status.Should().Be(PalletStatus.InStock);

			var inventory = await DbContext.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.Id);
			inventory.Should().NotBeNull();
			inventory.Quantity.Should().Be(10);
		}
	}
}
