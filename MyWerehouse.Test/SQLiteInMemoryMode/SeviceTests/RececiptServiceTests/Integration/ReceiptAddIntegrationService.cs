using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptAddIntegrationService : ReceiptIntegratioCommandService
	{
		//HappyPath
		[Fact]
		public async Task AddPalletToReceiptAsync_ProperData__AddedToBase()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialCategory = new Category
			{
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
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
			};
			var initialReceipt = new Receipt
			{
				Client = initialCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};			
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(initialLocation);
			DbContext.Clients.Add(initialCLient);
			DbContext.Receipts.Add(initialReceipt);
			
			await DbContext.SaveChangesAsync();
			//Act
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }],
				UserId = "U001"
			};

			var newPallet = await _receiptService.AddPalletToReceiptAsync(initialReceipt.Id, newPalletDto);
			//Assert
			Assert.NotNull(newPallet);
			var newPalletId = newPallet.PalletId;
			var palletFromDb = await DbContext.Pallets.FindAsync(newPalletId);
			Assert.NotNull(palletFromDb);
			Assert.Equal(initialReceipt.Id, palletFromDb.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, palletFromDb.Status);

			var productsOnPallet = DbContext.ProductOnPallet
				.Where(x => x.PalletId == palletFromDb.Id)
				.ToList();

			Assert.Single(productsOnPallet);
			Assert.Equal(1, productsOnPallet[0].ProductId);
			Assert.Equal(10, productsOnPallet[0].Quantity);

			var movement = DbContext.PalletMovements
				.FirstOrDefault(x => x.PalletId == newPalletId);

			Assert.NotNull(movement);
			Assert.Equal("U001", movement.PerformedBy);
			Assert.Equal(ReasonMovement.Received, movement.Reason);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.Id == initialReceipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.InProgress, historyRecipt.StatusAfter);

			var receipt = await DbContext.Receipts
				.Include(x => x.Pallets)
				.FirstOrDefaultAsync(x => x.Id == initialReceipt.Id);

			Assert.Contains(receipt.Pallets, p => p.Id == newPalletId);
		}

		[Fact]
		public async Task ProperData_CreateReceiptPlanAsync_AddToBase()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			DbContext.Clients.Add(initialCLient);
			DbContext.SaveChanges();
			//Act
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = initialCLient.Id,
				ReceiptDateTime = DateTime.UtcNow,
				PerformedBy = "user",				
			};
			var result =
				await _receiptService.CreateReceiptPlanAsync(newPalletDto);
			//Assert
			Assert.NotNull(result);
			var receipt = DbContext.Receipts.Find(result.ReceiptId);
			Assert.NotNull(receipt);
			Assert.Equal(ReceiptStatus.Planned, receipt.ReceiptStatus);
			Assert.Equal("user", receipt.PerformedBy);			
		}

		//SadPath
		[Fact]
		public async Task CreateReceiptPlanAsync_NoProperData_AddToBase()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			DbContext.Clients.Add(initialCLient);
			DbContext.SaveChanges();
			//Act
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = 2,
				//initialCLient.Id,
				ReceiptDateTime = DateTime.UtcNow,
				PerformedBy = "user",
			};
			var result =
				await _receiptService.CreateReceiptPlanAsync(newPalletDto);
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Wystąpił nieoczekiwany błąd podczas operacji.", result.Message);
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
				
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				Client = initialCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initialLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory = new Category
			{				
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
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

			var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
			_receiptService.AddPalletToReceiptAsync(initialReceipt.Id, newPalletDto));

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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				Client = initialCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initialLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
			};
			var product1 = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
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
