using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptAddIntegrationService : ReceiptIntegratioCommandService
	{
		[Fact]
		public async Task ProperDataOnePalletFullTest_AddPalletToReceiptAsync_AddedToBase()
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
			var initialCLient = new Client
			{
				//Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				//Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initialLocation = new Location
			{
				//Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var product = new Product
			{
				//Id = 1,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialCategory = new Category
			{
				//Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Clients.Add(initialCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }],
				UserId = "U001"
			};

			string newPallet = await _receiptService.AddPalletToReceiptAsync(initialReceipt.Id, newPalletDto);
			//Assert
			Assert.NotNull(newPallet);
			var palletFromDb = await DbContext.Pallets.FindAsync(newPallet);
			Assert.NotNull(palletFromDb);
			Assert.Equal(initialReceipt.Id, palletFromDb.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, palletFromDb.Status);

			var productsOnPallet = DbContext.ProductOnPallet
				.Where(x => x.PalletId == newPallet)
				.ToList();

			Assert.Single(productsOnPallet);
			Assert.Equal(1, productsOnPallet[0].ProductId);
			Assert.Equal(10, productsOnPallet[0].Quantity);

			var movement = DbContext.PalletMovements
				.FirstOrDefault(x => x.PalletId == newPallet);

			Assert.NotNull(movement);
			Assert.Equal("U001", movement.PerformedBy);
			Assert.Equal(ReasonMovement.Received, movement.Reason);

			var receipt = await DbContext.Receipts
				.Include(x => x.Pallets)
				.FirstOrDefaultAsync(x => x.Id == initialReceipt.Id);

			Assert.Contains(receipt.Pallets, p => p.Id == newPallet);
		}
		[Fact]
		public async Task NotProperDataProductQunatityZero_AddPalletToReceiptAsync_ThrowValidateException()
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
			var initialCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initialLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var product = new Product
			{
				Id = 1,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Clients.Add(initialCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = 0, Quantity = 0, }],
				UserId = "U001"
			};

			var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _receiptService.AddPalletToReceiptAsync(initialReceipt.Id, newPalletDto));

			Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
		}
		[Fact]
		public async Task NotProperDataTwoProduct_AddPalletToReceiptAsync_ThrowValidateException()
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
			var initialCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initialLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var product = new Product
			{
				Id = 1,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var product1 = new Product
			{
				Id = 2,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(product, product1);
			DbContext.Clients.Add(initialCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }, new() { ProductId = 2, Quantity = 100 }],
				UserId = "U001"
			};
			var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _receiptService.AddPalletToReceiptAsync(initialReceipt.Id, newPalletDto));

			Assert.Contains("Paleta przyjmowana może mieć tylko jeden rodzaj produktu", ex.Message);
		}
	}
}
