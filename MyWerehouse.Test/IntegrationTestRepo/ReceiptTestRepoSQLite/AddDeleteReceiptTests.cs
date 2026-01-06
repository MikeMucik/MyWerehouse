using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;
using MyWerehouse.Test.SQLiteInMemoryMode;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.IntegrationTestRepo.ReceiptTestRepoSQLite
{
	public class AddDeleteReceiptTests : TestBase
	{

		[Fact]
		public void AddReceipt_AddReceipt_AddToCollection()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
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
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2);
			var pallet1 = new Pallet
			{
				Id = "Q3000",
				DateReceived = DateTime.Now,
				Location = location1,
				Status = PalletStatus.Available,
			};
			var pallet2 = new Pallet
			{
				Id = "Q3001",
				DateReceived = DateTime.Now,
				Location = location2,
				Status = PalletStatus.Available,
			};
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var receiptRepo = new ReceiptRepo(DbContext);
			//Act	
			var receipt = new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			},
				PerformedBy = "U005"
			};
			receiptRepo.AddReceipt(receipt);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Receipts
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U005", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q3000");
			Assert.Contains(result.Pallets, p => p.Id == "Q3001");

			foreach (var item in result.Pallets)
			{
				Assert.Equal(receipt.Id, item.ReceiptId);
			}
		}
		[Fact]
		public void AddReceipt_AddReceiptPallletsStaff_AddToCollection()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
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
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2);
			var pallet1 = new Pallet
			{
				Id = "Q3000",
				DateReceived = DateTime.Now,
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet {
					Product = product, Quantity = 100, DateAdded = DateTime.Now, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
				}]
			};
			var pallet2 = new Pallet
			{
				Id = "Q3001",
				DateReceived = DateTime.Now,
				Location = location2,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet {
					Product = product, Quantity = 750, DateAdded = DateTime.Now, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(356))
				}]
			};
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var receiptRepo = new ReceiptRepo(DbContext);
			//Act	
			var receipt = new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			},
				PerformedBy = "U005"
			};
			receiptRepo.AddReceipt(receipt);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Receipts
				.Include(p => p.Pallets)
					.ThenInclude(pp=>pp.ProductsOnPallet)
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U005", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q3000");
			Assert.Contains(result.Pallets, p => p.Id == "Q3001");

			foreach (var pallet in result.Pallets)
			{
				Assert.Equal(receipt.Id, pallet.ReceiptId);
				Assert.Equal(PalletStatus.Available, pallet.Status);
				Assert.NotNull(pallet.Location);
				Assert.Single(pallet.ProductsOnPallet);
			}

			// --- Paleta 1: Q3000 ---
			var p1 = result.Pallets.First(p => p.Id == "Q3000");
			Assert.Equal("Q3000", p1.Id);
			Assert.Equal(1, p1.ProductsOnPallet.Count);

			var p1Prod = p1.ProductsOnPallet.First();
			Assert.Equal(100, p1Prod.Quantity);
			Assert.Equal(product.Id, p1Prod.ProductId);
			Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)), p1Prod.BestBefore);

			// --- Paleta 2: Q3001 ---
			var p2 = result.Pallets.First(p => p.Id == "Q3001");
			Assert.Equal("Q3001", p2.Id);
			Assert.Equal(1, p2.ProductsOnPallet.Count);

			var p2Prod = p2.ProductsOnPallet.First();
			Assert.Equal(750, p2Prod.Quantity);
			Assert.Equal(product.Id, p2Prod.ProductId);
			Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(356)), p2Prod.BestBefore);

			// --- Optional: sprawdzenie lokacji ---
			Assert.Equal(location1.Id, p1.LocationId);
			Assert.Equal(location2.Id, p2.LocationId);
		}
		//[Fact]
		//public void RemoveReceipt_DeleteReceipt_RemoveRecordFromList()
		//{
		//	//Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var initailClient = new Client
		//	{
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany",
		//		Addresses = new List<Address> { address }
		//	};
		//	var location1 = new Location
		//	{
		//		Aisle = 1,
		//		Bay = 1,
		//		Height = 1,
		//		Position = 1
		//	};
		//	var location2 = new Location
		//	{
		//		Aisle = 2,
		//		Bay = 1,
		//		Height = 1,
		//		Position = 1
		//	};
		//	var initialCategory = new Category
		//	{
		//		Name = "name",
		//		IsDeleted = false
		//	};
		//	var product = new Product
		//	{
		//		Name = "TestFull",
		//		SKU = "123",
		//		AddedItemAd = new DateTime(2024, 1, 1),
		//		Category = initialCategory,
		//		IsDeleted = false,
		//		CartonsPerPallet = 10,
		//	};
		//	DbContext.Clients.Add(initailClient);
		//	DbContext.Categories.Add(initialCategory);
		//	DbContext.Products.Add(product);
		//	DbContext.Locations.AddRange(location1, location2);
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q3000",
		//		DateReceived = DateTime.Now,
		//		Location = location1,
		//		Status = PalletStatus.Available,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q3001",
		//		DateReceived = DateTime.Now,
		//		Location = location2,
		//		Status = PalletStatus.Available,
		//	};
		//	DbContext.Pallets.AddRange(pallet1, pallet2);
		//	var receipt = new Receipt
		//	{
		//		ReceiptDateTime = DateTime.Now,
		//		Client = initailClient,
		//		Pallets = new List<Pallet>
		//	{
		//		pallet1,
		//		pallet2
		//	},
		//		PerformedBy = "U005"
		//	};
		//	DbContext.Receipts.Add(receipt);
		//	DbContext.SaveChanges();
		//	var receiptRepo = new ReceiptRepo(DbContext);
		//	//Act
		//	receiptRepo.DeleteReceipt(receipt);
		//	DbContext.SaveChanges();
		//	//Assert
		//	var receiptResult = DbContext.Receipts.Find(receipt.Id);
		//	Assert.Null(receiptResult);
		//}

	}
}
//[Fact]
//public void RemoveReceipt_DeleteReceipt_RemoveRecordFromList()
//{
//	//Arrange
//	var id = 1;
//	//Act
//	_receiptRepo.DeleteReceipt(id);
//	//Assert
//	var receipt = _context.Issues.Find(id);
//	Assert.Null(receipt);
//}
//[Fact]
//public async Task RemoveReceipt_DeleteReceiptAsync_RemoveRecordFromList()
//{
//	//Arrange
//	var id = 1;
//	//Act
//	await _receiptRepo.DeleteReceiptAsync(id);
//	//Assert
//	var receipt = _context.Issues.Find(id);
//	Assert.Null(receipt);
//}