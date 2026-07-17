using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ReceiptTestRepoSQLite
{
	public class AddDeleteReceiptTests : TestBase
	{

		[Fact]
		public void AddReceipt_AddToCollection()
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

			DbContext.Clients.Add(initailClient);
			DbContext.Locations.AddRange(location1);

			DbContext.SaveChanges();
			var receiptRepo = new ReceiptRepo(DbContext);

			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U005",
			new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			//Act
			receiptRepo.AddReceipt(receipt);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Receipts
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal("U005", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Equal(ReceiptStatus.Planned, result.ReceiptStatus);
		}
		
		[Fact]
		public void RemoveReceipt_DeleteReceipt_RemoveRecordFromList()
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
			var product = Product.Create("TestFull", "123", TestDates.UtcNow, 1, 10, 30, 30, 30, 30, "TestDetails");

			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2);

			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "U005",
			new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			DbContext.Receipts.Add(receipt);
			DbContext.SaveChanges();
			var pallet1 = Pallet.CreateForTests("Q3000", TestDates.Now, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 100, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("Q3001", TestDates.Now, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 750, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(356)));
			DbContext.Pallets.AddRange(pallet1, pallet2);		

			DbContext.SaveChanges();
			var receiptRepo = new ReceiptRepo(DbContext);
			//Act
			receiptRepo.DeleteReceipt(receipt);
			DbContext.SaveChanges();
			//Assert
			var receiptResult = DbContext.Receipts.Find(receipt.Id);
			Assert.Null(receiptResult);
		}

	}
}