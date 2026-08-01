using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.ReceiptTests.Integration
{
	public class ReceiptAddPalletToReceiptTests : TestBase
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
		public async Task AddPalletToReceipt_ShouldAddPalletToReceipt_WhenProperData()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var location = CreateLocation(1, 1);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);

			await DbContext.SaveChangesAsync();
			//Act
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = product.Id, Quantity = 10, }],
				UserId = "U001",
				ReceiptNumber = receipt.ReceiptNumber,
				ReceiptId = receipt.Id,
			};

			var result = await Mediator.Send(new AddPalletToReceiptCommand(receipt.Id, newPalletDto));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var newPallet = DbContext.Pallets.FirstOrDefault(p => p.ReceiptId == receipt.Id);
			Assert.NotNull(newPallet);
			var palletFromDb = newPallet;
			Assert.NotNull(palletFromDb);
			Assert.Equal(receipt.Id, palletFromDb.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, palletFromDb.Status);

			var productsOnPallet = DbContext.ProductOnPallet
				.Where(x => x.PalletId == palletFromDb.Id)
				.ToList();

			Assert.Single(productsOnPallet);
			Assert.Equal(product.Id, productsOnPallet[0].ProductId);
			Assert.Equal(10, productsOnPallet[0].Quantity);

			var movement = DbContext.HistoryPallet
				.FirstOrDefault(x => x.PalletId == newPallet.Id);

			Assert.NotNull(movement);
			Assert.Equal("U001", movement.PerformedBy);
			Assert.Equal(ReasonForPallet.Received, movement.Reason);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == receipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.InProgress, historyRecipt.StatusAfter);

			var receiptAfter = await DbContext.Receipts
				.Include(x => x.Pallets)
				.FirstOrDefaultAsync(x => x.Id == receipt.Id);
			Assert.NotNull(receiptAfter);
			Assert.Contains(receiptAfter.Pallets, p => p.Id == newPallet.Id);
		}
		[Fact]
		public async Task AddPalletToReceipt_ThrowValidateException_WhenProductQunatityZero()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var location = CreateLocation(1, 1);;
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(location);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = product.Id, Quantity = 0, }],
				UserId = "U001"
			};

			var ex = await Assert.ThrowsAsync<ValidationException>(() =>
			Mediator.Send(new AddPalletToReceiptCommand(receipt.Id, newPalletDto)));

			Assert.Contains("Product quantity must be greater than zero.", ex.Message);
		}
		[Fact]
		public async Task AddPalletToReceipt_ThrowValidateException_WhenTwoProduct()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test1", "777777");
			var location = CreateLocation(1, 1);			
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(location);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = product.Id, Quantity = 10, }, new() { ProductId = product1.Id, Quantity = 100 }],
				UserId = "U001"
			};
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new AddPalletToReceiptCommand(receipt.Id, newPalletDto)));

			Assert.Contains("A received pallet may contain only one product type.", ex.Message);
		}
	}
}
