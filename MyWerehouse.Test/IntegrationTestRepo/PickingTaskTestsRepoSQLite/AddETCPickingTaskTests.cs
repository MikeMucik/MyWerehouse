using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PickingTaskTestsRepoSQLite
{
	public class AddETCPickingTaskTests :TestBase
	{
		[Fact]
		public void AddNewRecord_AddPickingTask_AddToCollection()
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
			var product = Product.Create("Banana", "1234567890", 1, 56);
			
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var pallet = Pallet.CreateForTests("Q00001", DateTime.Now, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)));
			
			DbContext.Clients.Add(initailClient);
			DbContext.Products.Add(product);
			DbContext.Categories.Add(newCategory);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			var virtualPallet = VirtualPallet.Create(pallet.Id, pallet.ProductsOnPallet.First().Quantity, pallet.LocationId);
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, new DateTime(2025, 5, 5)
				, new DateTime(2025, 5, 6), "U002",IssueStatus.Pending, null);
			
			DbContext.Issues.Add(issue);
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 10, PickingStatus.Allocated, product.Id,
				null, null, null, 0);
			var pickingTaskRepo = new PickingTaskRepo(DbContext);
			//Act
			pickingTaskRepo.AddPickingTask(pickingTask);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.PickingTasks.Find(pickingTask.Id);
			Assert.NotNull(result);
			Assert.Equal(pickingTask.Issue, result.Issue);
			Assert.Equal(pickingTask.VirtualPallet, result.VirtualPallet);
			Assert.Equal(pickingTask.RequestedQuantity, result.RequestedQuantity);
		}
		[Fact]
		public void DeleteRecord_DeletePickingTask_ReomveFromCollection()
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
			var product = Product.Create("Banana", "1234567890", 1, 56);
			
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var pallet = Pallet.CreateForTests("Q00001", DateTime.Now, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)));
			
			DbContext.Products.Add(product);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(newCategory);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			var virtualPallet = VirtualPallet.Create(pallet.Id, pallet.ProductsOnPallet.First().Quantity, pallet.LocationId);
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 2, 1, new DateTime(2025, 5, 5)
				, new DateTime(2025, 5, 6), "U002", IssueStatus.Pending, null);
		
			DbContext.Issues.Add(issue);
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 10, PickingStatus.Allocated, product.Id,
				null, null, null, 0);
			var pickingTaskRepo = new PickingTaskRepo(DbContext);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			//Act
			pickingTaskRepo.DeletePickingTask(pickingTask);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.PickingTasks.Find(pickingTask.Id);
			Assert.Null(result);
			
		}
	}
}
