using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.AllocationTestsRepoSQLite
{
	public class AddETCAllocationTests :TestBase
	{
		[Fact]
		public void AddNewRecord_AddAllocation_AddToCollection()
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
			var newCategory = new Category
			{
				Name = "CategoryName"
			};			
			var product = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> {
					new ProductOnPallet
					{
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddMonths(-1),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
					}
				}
			};
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(newCategory);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet,
				LocationId = pallet.LocationId,
				IssueInitialQuantity = pallet.ProductsOnPallet.First().Quantity,
				DateMoved = DateTime.Now,
				Allocations = new List<Allocation>()

			};
			var issue = new Issue
			{
				//Id = 2,
				Client = initailClient,
				PerformedBy = "U002",
				IssueDateTimeCreate = new DateTime(2025, 5, 5),
				IssueDateTimeSend = new DateTime(2025, 5, 6),//zmiana 
			};
			DbContext.Issues.Add(issue);
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.SaveChanges();
			var allocation = new Allocation
			{
				Issue = issue,
				VirtualPallet = virtualPallet,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated,
			};
			var allocationRepo = new AllocationRepo(DbContext);
			//Act
			allocationRepo.AddAllocation(allocation);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Allocations.Find(allocation.Id);
			Assert.NotNull(result);
			Assert.Equal(allocation.Issue, result.Issue);
			Assert.Equal(allocation.VirtualPallet, result.VirtualPallet);
			Assert.Equal(allocation.Quantity, result.Quantity);
		}
		[Fact]
		public void DeleteRecord_DeleteAllocation_ReomveFromCollection()
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
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			var product = new Product
			{
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
				CartonsPerPallet = 56,
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> {
					new ProductOnPallet
					{
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddMonths(-1),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
					}
				}
			};
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(newCategory);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet,
				LocationId = pallet.LocationId,
				IssueInitialQuantity = pallet.ProductsOnPallet.First().Quantity,
				DateMoved = DateTime.Now,
				Allocations = new List<Allocation>()

			};
			var issue = new Issue
			{
				//Id = 2,
				Client = initailClient,
				PerformedBy = "U002",
				IssueDateTimeCreate = new DateTime(2025, 5, 5),
				IssueDateTimeSend = new DateTime(2025, 5, 6),//zmiana 
			};
			DbContext.Issues.Add(issue);
			DbContext.VirtualPallets.Add(virtualPallet);
			
			var allocation = new Allocation
			{
				Issue = issue,
				VirtualPallet = virtualPallet,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated,
			};
			var allocationRepo = new AllocationRepo(DbContext);
			DbContext.Allocations.Add(allocation);
			DbContext.SaveChanges();
			//Act
			allocationRepo.DeleteAllocation(allocation);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Allocations.Find(allocation.Id);
			Assert.Null(result);
			
		}
	}
}
