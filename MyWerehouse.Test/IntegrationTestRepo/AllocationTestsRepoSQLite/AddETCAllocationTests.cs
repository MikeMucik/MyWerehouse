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
			//var product = new Product
			//{
			//	Name = "Banana",
			//	SKU = "1234567890",
			//	CategoryId = 1,
			//	CartonsPerPallet = 56,
			//};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var pallet = Pallet.CreateForTests("Q00001", DateTime.Now, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)));
			//var pallet = new Pallet
			//{
			//	PalletNumber = "Q00001",
			//	DateReceived = DateTime.Now,
			//	LocationId = 1,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> {
			//		new ProductOnPallet
			//		{
			//			Product = product,
			//			Quantity = 10,
			//			DateAdded = DateTime.UtcNow.AddMonths(-1),
			//			BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
			//		}
			//	}
			//};
			DbContext.Clients.Add(initailClient);
			DbContext.Products.Add(product);
			DbContext.Categories.Add(newCategory);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet,
				LocationId = pallet.LocationId,
				InitialPalletQuantity = pallet.ProductsOnPallet.First().Quantity,
				DateMoved = DateTime.Now,
				PickingTasks = new List<PickingTask>()

			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = initailClient,
				PerformedBy = "U002",
				IssueDateTimeCreate = new DateTime(2025, 5, 5),
				IssueDateTimeSend = new DateTime(2025, 5, 6),//zmiana 
			};
			DbContext.Issues.Add(issue);
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.SaveChanges();
			var pickingTask = new PickingTask
			{
				Issue = issue,
				VirtualPallet = virtualPallet,
				RequestedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
			};
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
			//var product = new Product
			//{
			//	Name = "Banana",
			//	SKU = "1234567890",
			//	CategoryId = 1,
			//	CartonsPerPallet = 56,
			//};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var pallet = Pallet.CreateForTests("Q00001", DateTime.Now, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)));
			//var pallet = new Pallet
			//{
			//	PalletNumber = "Q00001",
			//	DateReceived = DateTime.Now,
			//	LocationId = 1,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> {
			//		new ProductOnPallet
			//		{
			//			Product = product,
			//			Quantity = 10,
			//			DateAdded = DateTime.UtcNow.AddMonths(-1),
			//			BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
			//		}
			//	}
			//};
			//var pallet = new Pallet
			//{
			//	PalletNumber = "Q00001",
			//	DateReceived = DateTime.Now,
			//	LocationId = 1,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet> {
			//		new ProductOnPallet
			//		{
			//			Product = product,
			//			Quantity = 10,
			//			DateAdded = DateTime.UtcNow.AddMonths(-1),
			//			BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
			//		}
			//	}
			//};
			DbContext.Products.Add(product);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(newCategory);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet,
				LocationId = pallet.LocationId,
				InitialPalletQuantity = pallet.ProductsOnPallet.First().Quantity,
				DateMoved = DateTime.Now,
				PickingTasks = new List<PickingTask>()

			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),	
				IssueNumber = 2,
				Client = initailClient,
				PerformedBy = "U002",
				IssueDateTimeCreate = new DateTime(2025, 5, 5),
				IssueDateTimeSend = new DateTime(2025, 5, 6),//zmiana 
			};
			DbContext.Issues.Add(issue);
			DbContext.VirtualPallets.Add(virtualPallet);
			
			var pickingTask = new PickingTask
			{
				Issue = issue,
				VirtualPallet = virtualPallet,
				RequestedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
			};
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
