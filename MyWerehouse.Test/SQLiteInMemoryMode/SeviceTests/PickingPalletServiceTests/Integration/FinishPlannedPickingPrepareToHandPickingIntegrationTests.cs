using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.PickingPallets.Commands.FinishPlannedPickingPrepareToHandPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class FinishPlannedPickingPrepareToHandPickingIntegrationTests : TestBase
	{
		[Fact]
		public async Task FinishPlannedPickingPrepareToHandPicking_CancelPickingTask_CreateHandPicking()
		{
			// Arrange
			var category = new Category
			{	Id =1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A","666",1,100);
			
			var product2 = Product.Create("Prod B", "777", 1, 100);
			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var sourcePallet2 = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var sourcePallet3 = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet3.AddProductForTests(product1.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddHours(-12), "TestUser", IssueStatus.New, null);

			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid1 = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid1, 1, issue.Id, 10, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			//var pickingTask1 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))
			//};
			var pickingGuid2 = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid2, 2, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			//var pickingTask2 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product2.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))
			//};
			var pickingGuid3 = Guid.NewGuid();
			var pickingTask3 = PickingTask.CreateForSeed(pickingGuid3, 3, issue.Id, 15, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			//var pickingTask3 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 15,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			//};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet2,
				InitialPalletQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask1 }
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			var virtualPallet3 = new VirtualPallet
			{
				Pallet = sourcePallet3,
				InitialPalletQuantity = 15,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask3 }
			};
			//pickingTask2.VirtualPallet = virtualPallet2;
			//pickingTask3.VirtualPallet = virtualPallet3;
			//pickingTask1.VirtualPallet = virtualPallet1;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			await DbContext.SaveChangesAsync();
			//Act 
			var result = Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand("User"));
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Result.Count);
			var resultForProduct1 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product1.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			var resultForProduct2 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product2.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			Assert.NotNull(resultForProduct1);
			Assert.NotNull(resultForProduct2);
			Assert.Equal(25, resultForProduct1.RequestedQuantity);
			Assert.Equal(10, resultForProduct2.RequestedQuantity);
		}
		[Fact]
		public async Task FinishPlannedPickingPartialDonePrepareToHandPicking_CancelPickingTask_CreateHandPicking()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);
			
			var product2 = Product.Create("Prod B", "777", 1, 100);
			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var sourcePallet2 = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var sourcePallet3 = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet3.AddProductForTests(product1.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddHours(-12), "TestUser", IssueStatus.New, null);

			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid1 = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid1, 1, issue.Id, 10, PickingStatus.PickedPartially, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 5);
			//var pickingTask1 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickedQuantity = 5,
			//	PickingStatus = PickingStatus.PickedPartially,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))
			//};
			var pickingGuid2 = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid2, 2, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			//var pickingTask2 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product2.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))
			//};
			var pickingGuid3 = Guid.NewGuid();
			var pickingTask3 = PickingTask.CreateForSeed(pickingGuid3, 3, issue.Id, 15, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			//var pickingTask3 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 15,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			//};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet2,
				InitialPalletQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask1 }
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			var virtualPallet3 = new VirtualPallet
			{
				Pallet = sourcePallet3,
				InitialPalletQuantity = 15,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask3 }
			};
			//pickingTask2.VirtualPallet = virtualPallet2;
			//pickingTask3.VirtualPallet = virtualPallet3;
			//pickingTask1.VirtualPallet = virtualPallet1;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			await DbContext.SaveChangesAsync();
			//Act 
			var result = Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand("user"));
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Result.Count);
			var resultForProduct1 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product1.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			var resultForProduct2 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product2.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			Assert.NotNull(resultForProduct1);
			Assert.NotNull(resultForProduct2);
			Assert.Equal(20, resultForProduct1.RequestedQuantity);
			Assert.Equal(10, resultForProduct2.RequestedQuantity);
		}
	}
}
