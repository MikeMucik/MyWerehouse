using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Application.Receipts.Commands.CreateReceipt;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptAddIntegrationTests : TestBase
	{
		//HappyPath
		[Fact]
		public async Task AddPalletToReceiptAsync_ProperData_AddedToBase()
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
			var product = Product.Create("Test", "666666", 1, 56);
			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			//var initialReceipt = new Receipt
			//{
			//	//Id = Guid.NewGuid(),
			//	ReceiptNumber = 1,
			//	Client = initialCLient,
			//	ReceiptStatus = ReceiptStatus.Planned,
			//	PerformedBy = "U002",
			//	RampNumber = 1
			//};			
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(initialLocation);
			DbContext.Clients.Add(initialCLient);
			DbContext.Receipts.Add(initialReceipt);
			
			await DbContext.SaveChangesAsync();
			//Act
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = product.Id, Quantity = 10, }],
				UserId = "U001",
				ReceiptNumber = initialReceipt.ReceiptNumber,
				ReceiptId = initialReceipt.Id,
			};
						
			var result = await Mediator.Send(new AddPalletToReceiptCommand(initialReceipt.Id, newPalletDto));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			//var newPalletId = newPallet.PalletId;
			var newPallet = DbContext.Pallets.FirstOrDefault(p => p.ReceiptId == initialReceipt.Id);
			var palletFromDb = newPallet;
				//await DbContext.Pallets.FindAsync(newPalletId);
			Assert.NotNull(palletFromDb);
			Assert.Equal(initialReceipt.Id, palletFromDb.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, palletFromDb.Status);

			var productsOnPallet = DbContext.ProductOnPallet
				.Where(x => x.PalletId == palletFromDb.Id)
				.ToList();

			Assert.Single(productsOnPallet);
			Assert.Equal(product.Id, productsOnPallet[0].ProductId);
			Assert.Equal(10, productsOnPallet[0].Quantity);

			var movement = DbContext.PalletMovements
				.FirstOrDefault(x => x.PalletId == newPallet.Id);

			Assert.NotNull(movement);
			Assert.Equal("U001", movement.PerformedBy);
			Assert.Equal(ReasonMovement.Received, movement.Reason);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == initialReceipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.InProgress, historyRecipt.StatusAfter);

			var receipt = await DbContext.Receipts
				.Include(x => x.Pallets)
				.FirstOrDefaultAsync(x => x.Id == initialReceipt.Id);

			Assert.Contains(receipt.Pallets, p => p.Id == newPallet.Id);
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
			var location = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 1
			};
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(initialCLient);
			DbContext.SaveChanges();
			//Act
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = initialCLient.Id,
				ReceiptDateTime = DateTime.UtcNow,
				PerformedBy = "user",
				RampNumber = 1
			};
			var result = await Mediator.Send(new CreateReceiptPlanCommand(newPalletDto));
			//Assert
			Assert.NotNull(result);
			var receipt = DbContext.Receipts.FirstOrDefault(x => x.ClientId == initialCLient.Id);
			//var receipt = DbContext.Receipts.FirstOrDefault(x => x.ReceiptDateTime ==newPalletDto.ReceiptDateTime);
			Assert.NotNull(receipt);
			Assert.Equal(ReceiptStatus.Planned, receipt.ReceiptStatus);
			Assert.Equal("user", receipt.PerformedBy);			
		}

		//SadPath
		[Fact]
		public async Task CreateReceiptPlanAsync_NoUser_NotAddToBase()
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
			var location = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 1
			};
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(initialCLient);
			DbContext.SaveChanges();
			//Act
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = initialCLient.Id,
				ReceiptDateTime = DateTime.UtcNow,
				//PerformedBy = "user",
				RampNumber = 1
			};
			var result =
				await Mediator.Send(new CreateReceiptPlanCommand(newPalletDto));
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Invalid  or missing user ID.", result.Error);
		}
		[Fact]
		public async Task CreateReceiptPlanAsync_NoProperData_NotAddToBase()
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
				ReceiptDateTime = DateTime.UtcNow,
				PerformedBy = "user",
				RampNumber = 1
			};
			var result =
				await Mediator.Send(new CreateReceiptPlanCommand(newPalletDto));
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Klient o numerze 2 nie istnieje.", result.Error);
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
			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			//var initialReceipt = new Receipt
			//	{
			//		Id = Guid.NewGuid(),
			//		ReceiptNumber = 1,
			//		Client = initialCLient,
			//		ReceiptStatus = ReceiptStatus.Planned,
			//		PerformedBy = "U002"
			//	};
				var initialLocation = new Location
				{
					Aisle = 1,
					Bay = 1,
					Height = 1,
					Position = 1
				};
				var initialCategory = new Category
				{		
					Id =1,
					Name = "name",
					IsDeleted = false
				};
			var product = Product.Create("Test", "666666", 1, 56);
			
				DbContext.Categories.Add(initialCategory);
				DbContext.Products.Add(product);
				DbContext.Clients.Add(initialCLient);
				DbContext.Receipts.Add(initialReceipt);
				DbContext.Locations.Add(initialLocation);
				await DbContext.SaveChangesAsync();
				//Act&Assert
				var newPalletDto = new CreatePalletReceiptDTO
				{
					ProductsOnPallet = [new() { ProductId = product.Id, Quantity = 0, }],
					UserId = "U001"
				};

				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
				Mediator.Send(new AddPalletToReceiptCommand(initialReceipt.Id, newPalletDto)));

				Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
				//Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
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
			var initialReceipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			//var initialReceipt = new Receipt
			//{
			//	Id = Guid.NewGuid(),
			//	ReceiptNumber = 1,
			//	Client = initialCLient,
			//	ReceiptStatus = ReceiptStatus.Planned,
			//	PerformedBy = "U002"
			//};
			var initialLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory = new Category
			{Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("Test", "666666", 1, 56);
			
			var product1 = Product.Create("Test", "666666", 1, 56);
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(product, product1);
			DbContext.Clients.Add(initialCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = product.Id, Quantity = 10, }, new() { ProductId = product1.Id, Quantity = 100 }],
				UserId = "U001"
			};
			var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => Mediator.Send(new AddPalletToReceiptCommand(initialReceipt.Id, newPalletDto)));

			Assert.Contains("Paleta przyjmowana może mieć tylko jeden rodzaj produktu", ex.Message);
		}
	}
}
