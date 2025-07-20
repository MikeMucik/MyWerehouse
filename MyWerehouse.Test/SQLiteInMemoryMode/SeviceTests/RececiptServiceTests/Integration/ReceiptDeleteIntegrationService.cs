using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptDeleteIntegrationService : ReceiptIntegratioCommandService
	{
		[Fact]
		public async Task NotVerifiedReceipt_DeleteReceiptAsync_RemoveFromBase()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 1,
			};
			var initialPallet1 = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 1,
			};
			var initialProductOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialProductOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q2000",
				ProductId = 1,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets = [initialPallet]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
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
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			
			//Act
			await _receiptService.DeleteReceiptAsync(initialReceipt.Id);
			//Assert		

			Assert.Empty(DbContext.Receipts);
			Assert.Empty(DbContext.Pallets);
			Assert.Empty(DbContext.ProductOnPallet);

			Assert.Equal(2, DbContext.Products.Count());
			Assert.Equal(1, DbContext.Categories.Count());
			Assert.Equal(1, DbContext.Clients.Count());
		}
		[Fact]
		public async Task VerifiedReceipt_DeleteReceiptAsync_ThrowException()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 1,
			};
			var initialPallet1 = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 1,
			};
			var initialProductOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialProductOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q2000",
				ProductId = 1,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets = [initialPallet]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
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
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			//Act&Assert			
			var ex = await Assert.ThrowsAsync<InvalidDataException>(() => _receiptService.DeleteReceiptAsync(initialReceipt.Id));
			Assert.Contains("Nie można usunąć zweryfikowanego przyjęcia", ex.Message);
		}
	}
}
