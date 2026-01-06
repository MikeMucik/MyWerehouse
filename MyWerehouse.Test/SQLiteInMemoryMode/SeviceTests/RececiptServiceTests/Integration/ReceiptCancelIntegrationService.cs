using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptCancelIntegrationService : ReceiptIntegratioCommandService
	{
		//Zmiana metody z delete na cancel
		[Fact]
		public async Task DeleteReceiptAsync_NotVerifiedReceipt_CancelledInBase()
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
			var result = await _receiptService.CancelReceiptAsync(initialReceipt.Id, "user");
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Anulowano przyjęcie wraz z paletami z bazy", result.Message);
			var receipt = DbContext.Receipts.FirstOrDefault(receipt => receipt.Id == initialReceipt.Id);
			Assert.NotNull(receipt);
			Assert.Equal(ReceiptStatus.Cancelled, receipt.ReceiptStatus);
			//Assert.Empty(DbContext.Receipts);
			Assert.NotNull(DbContext.Pallets);
			var pallets = DbContext.Pallets.Where(p=>p.ReceiptId == initialReceipt.Id).ToList();
			foreach (var palet in pallets)
			{
				Assert.Equal(PalletStatus.Cancelled, palet.Status);
			}
			Assert.NotNull(DbContext.ProductOnPallet);

			Assert.Equal(2, DbContext.Products.Count());
			Assert.Equal(1, DbContext.Categories.Count());
			Assert.Equal(1, DbContext.Clients.Count());
		}
		[Fact]
		public async Task DeleteReceiptAsync_NotVerifiedReceipt_RemovedBase()
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
			var client = new Client
			{				
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};			
			var initialReceipt = new Receipt
			{
				Client = client,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
			};			
			DbContext.Clients.Add(client);
			DbContext.Receipts.Add(initialReceipt);		
			await DbContext.SaveChangesAsync();
			//Act
			var result = await _receiptService.CancelReceiptAsync(initialReceipt.Id, "user");
			//Assert	
			Assert.NotNull(result);
			Assert.Contains("Usunięto zlecenie", result.Message);
		}
			[Fact]
		public async Task DeleteReceiptAsync_VerifiedReceipt_ThrowInfo()
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
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
			};
			var initialReceipt = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),				
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initialLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
			};
			var initialPallet1 = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = initialLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
			};
			var initialProductOnPallet = new ProductOnPallet
			{				
				PalletId = "Q1000",
				Product = initialProduct,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialProductOnPallet1 = new ProductOnPallet
			{				
				PalletId = "Q2000",
				Product = initialProduct1,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};	
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initialLocation);
			await DbContext.SaveChangesAsync();
			//Act&Assert		
			var result = await _receiptService.CancelReceiptAsync(initialReceipt.Id, "user");
			Assert.NotNull(result);
			Assert.Contains("Nie można usunąć zweryfikowanego przyjęcia", result.Message);		
		}
	}
}
