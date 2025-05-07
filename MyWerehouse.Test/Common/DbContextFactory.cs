using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.Common
{
	public class DbContextFactory
	{
		public static Mock<WerehouseDbContext> Create()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var mock = new Mock<WerehouseDbContext>(options) { CallBase = true };
			var context = mock.Object;
			context.Database.EnsureCreated();
			SeedDatabase(context);
			return mock;
		}
		public static void SeedDatabase(WerehouseDbContext context)
		{//tu dane do bazy InMemory
			if (!context.Categories.Any())
			{
				var category = new Category
				{
					Id = 1,
					Name = "TestCategory",
				};
				context.Add(category);
				var category1 = new Category
				{
					Id = 2,
					Name = "TestCategory1",
				};
				context.Add(category1);
			}
			if (!context.Products.Any())
			{
				var product = new Product
				{
					Id = 10,
					Name = "Test",
					SKU = "0987654321",
					CategoryId = 1,
					IsDeleted = false,
					//ReceiptList = new 
				};
				context.Add(product);
				var productToD = new Product
				{
					Id = 11,
					Name = "TestD",
					SKU = "fghtredfg",
					CategoryId = 1,
					IsDeleted = false,
				};
				context.Add(productToD);
			}
			if (!context.Receipts.Any()) 
			{
				var receipt = new Receipt
				{
					Id = 1,
					ClientId = 10,
					PerformedBy = "U001",
					//Pallets = List<>
				};
				context.Add(receipt);
			}
			if (!context.ProductDetails.Any())
			{
				var productD = new ProductDetails
				{
					Id = 10,
					ProductId = 10,
					Length = 10,
					Height = 20,
					Width = 30,
					Weight = 2,
					Description = "TestDetails",
					
				};
				context.Add(productD);
			}
			if (!context.Clients.Any())
			{
				var client = new Client
				{
					Id = 10,
					Name = "ClientTest",
					Email = "client@op.pl",
					Description = "ClientDescription",
					FullName = "FullNameTestAddress",
				};
				context.Add(client);
				var client1 = new Client
				{
					Id = 11,
					Name = "ClientTest1",
					Email = "client1@op.pl",
					Description = "ClientDescription1",
					FullName = "FullNameTestAddress1"
				};
				context.Add(client1);
			}
			if (!context.Adresses.Any())
			{
				var address = new Address
				{
					Id = 10,
					//FullName = "FullNameTestAddress",
					Country = "ConutryTest",
					City = "CityTest",
					Region = "RegionTest",
					Phone = 123123123,
					PostalCode = "12ggt",
					StreetName = "StreetTest",
					StreetNumber = "12/1",
					ClientId = 10,
				};
				context.Add(address);
				var address1 = new Address
				{
					Id = 11,
					//FullName = "FullNameTestAddress1",
					Country = "ConutryTest1",
					City = "CityTest1",
					Region = "RegionTest1",
					Phone = 987987987,
					PostalCode = "test12ggt",
					StreetName = "StreetTest1",
					StreetNumber = "12/11",
					ClientId = 10,
				};
				context.Add(address1);
			}
			if (!context.Pallets.Any())
			{
				var pallet = new Pallet
				{
					Id = "Q1000",
					DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
					LocationId = 1,
					Status = PalletStatus.Available
				};
				context.Add(pallet);
				var pallet1 = new Pallet
				{
					Id = "Q1001",
					DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
					LocationId = 2,
					Status = PalletStatus.Available
				};
				context.Add(pallet1);
			}
			if (!context.ProductOnPallet.Any())
			{
				var productOnPallet = new ProductOnPallet
				{
					Id = 1,
					ProductId = 10,
					Quantity = 10,
					BestBefore = new DateOnly(2024, 2, 2),
					DateAdded = new DateTime(2026, 2, 2, 0, 0, 0),
					PalletId = "Q1000"
				};
				context.Add(productOnPallet);
				var productOnPallet1 = new ProductOnPallet
				{
					Id = 2,
					ProductId = 10,
					Quantity = 100,
					BestBefore = new DateOnly(2024, 2, 2),
					DateAdded = new DateTime(2026, 2, 2, 0, 0, 0),
					PalletId = "Q1001"
				};
				context.Add(productOnPallet1);
			}
			if (!context.Locations.Any())
			{
				var location = new Location
				{
					Id = 1,
					Aisle = 1,
					Bay = 2,
					Position = 3,
					Height = 4,
				};
				context.Add(location);
			}
			if (!context.PalletMovement.Any())
			{
				var movement = new PalletMovement
				{
					Id = 1,
					PalletId = "Q1000",
					ProductId = 10,
					LocationId = 2,
					Reason = ReasonMovement.ManualMove,
					Quantity = 1,
					MovementDate = new DateTime(2025,2,2,0,0,0)
				};
				context.Add(movement);
			}
			context.SaveChanges();
		}
		public static void Destroy(WerehouseDbContext context)
		{
			context.Database.EnsureDeleted();
			context.Dispose();
		}
	}
}
