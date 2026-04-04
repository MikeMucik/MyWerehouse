using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class ExecutiveHandPickingIntegrationTest : TestBase
	{
		[Fact]
		public async Task ExecutiveHandPicking_TaskAllNeededNoVirtualPallet_AddToIssue()
		{
			// Arrange
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
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPickingZone = new Location
			{
				Id = 100100,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 0);
			//var handPicknigTask = new PickingTask
			//{
			//	IssueId = issue.Id,
			//	//CreateDate = DateTime.UtcNow.AddDays(0),				
			//	PickingStatus = PickingStatus.Available,
			//	RequestedQuantity = 20,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(300)),
			//};
			DbContext.PickingTasks.Add(handPicknigTask);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 20, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			var pallets = DbContext.Pallets.Where(p => p.IssueId == issue.Id).ToList();
			Assert.Equal(2, pallets.Count);
			Assert.Contains(pallets, p => p.PalletNumber == "Q1001"); // pierwotna
			Assert.Contains(pallets, p => p.PalletNumber == "Q1002"); // ręczna

			Assert.Contains("Towar dołączono", result.Message);
			var handTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id);

			Assert.Equal(PickingStatus.Picked, handTask.PickingStatus);
			Assert.Equal(20, handTask.RequestedQuantity);

			var virtualPallet = DbContext.VirtualPallets
			.Include(v => v.PickingTasks)
			.Single(v => v.PalletId == sourcePallet1.Id);

			Assert.NotNull(virtualPallet);
			Assert.Equal(sourcePallet1.Id, virtualPallet.PalletId);
			Assert.Single(virtualPallet.PickingTasks);

			//var pickingTask = virtualPallet.PickingTasks.Single();
			var pickingTask = DbContext.PickingTasks.FirstOrDefault();
			Assert.Equal(issue.Id, pickingTask.IssueId);
			Assert.Equal(product1.Id, pickingTask.ProductId);
			Assert.Equal(20, pickingTask.RequestedQuantity);
			Assert.Equal(PickingStatus.Picked, pickingTask.PickingStatus);
			Assert.Equal(sourcePallet1.Id, pickingTask.VirtualPallet.PalletId);

			var palletsAdded = DbContext.Pallets
				.Where(p => p.IssueId == issue.Id)
				.ToList();

			var sourcePalletAfter = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(p => p.PalletNumber == "Q1000");

			Assert.Equal(80, sourcePalletAfter.ProductsOnPallet.Single().Quantity);

			var plannedTasks = DbContext.PickingTasks
				.Where(t => t.IssueId == issue.Id && t.PickingStatus == PickingStatus.Allocated)
				.ToList();

			Assert.Empty(plannedTasks);
			Assert.True(DbContext.HistoryPickings.Any(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id));

		}
		[Fact]
		public async Task ExecutiveHandPicking_TaskAllNeededWithVirtualPallet_AddToIssue()
		{
			// Arrange
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
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);
			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPickingZone = new Location
			{
				Id = 100100,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 0);
			//var handPicknigTask = new PickingTask
			//{
			//	IssueId = issue.Id,
			//	//CreateDate = DateTime.UtcNow.AddDays(-1),
			//	RequestedQuantity = 20,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(300)),
			//	PickingStatus = PickingStatus.Available,
			//};

			var virtualPallet = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 100,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = []
			};
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.PickingTasks.Add(handPicknigTask);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 20, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var pallets = DbContext.Pallets.Where(p => p.IssueId == issue.Id).ToList();
			Assert.Equal(2, pallets.Count);
			Assert.Contains(pallets, p => p.PalletNumber == "Q1001"); // pierwotna
			Assert.Contains(pallets, p => p.PalletNumber == "Q1002"); // ręczna

			Assert.Contains("Towar dołączono", result.Message);
			var handTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id);

			Assert.Equal(PickingStatus.Picked, handTask.PickingStatus);
			Assert.Equal(20, handTask.RequestedQuantity);

			var virtualPalletOld = DbContext.VirtualPallets
			.Include(v => v.PickingTasks)
			.Single(v => v.PalletId == sourcePallet1.Id);

			Assert.NotNull(virtualPalletOld);
			Assert.Equal(sourcePallet1.Id, virtualPalletOld.PalletId);
			Assert.Single(virtualPalletOld.PickingTasks);

			var pickingTask = virtualPalletOld.PickingTasks.Single();

			Assert.Equal(issue.Id, pickingTask.IssueId);
			Assert.Equal(product1.Id, pickingTask.ProductId);
			Assert.Equal(20, pickingTask.RequestedQuantity);
			Assert.Equal(PickingStatus.Picked, pickingTask.PickingStatus);
			Assert.Equal(sourcePallet1.Id, pickingTask.VirtualPallet.PalletId);

			var palletsAdded = DbContext.Pallets
				.Where(p => p.IssueId == issue.Id)
				.ToList();

			var sourcePalletAfter = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(p => p.PalletNumber == "Q1000");

			Assert.Equal(80, sourcePalletAfter.ProductsOnPallet.Single().Quantity);

			var plannedTasks = DbContext.PickingTasks
				.Where(t => t.IssueId == issue.Id && t.PickingStatus == PickingStatus.Allocated)
				.ToList();

			Assert.Empty(plannedTasks);
			Assert.True(DbContext.HistoryPickings.Any(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id));
		}
		[Fact]
		public async Task ExecutiveHandPicking_TaskPartialNeededNoVirtualPallet_AddToIssue()
		{
			// Arrange
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
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);
			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPickingZone = new Location
			{
				Id = 100100,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 0);
			//var handPicknigTask = new PickingTask
			//{
			//	IssueId = issue.Id,
			//	PickingStatus = PickingStatus.Available,
			//	//CreateDate = DateTime.UtcNow.AddDays(-1),
			//	RequestedQuantity = 20,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(300)),
			//};
			DbContext.PickingTasks.Add(handPicknigTask);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 12, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var pallets = DbContext.Pallets.Where(p => p.IssueId == issue.Id).ToList();
			Assert.Equal(2, pallets.Count);
			Assert.Contains(pallets, p => p.PalletNumber == "Q1001"); // pierwotna
			Assert.Contains(pallets, p => p.PalletNumber == "Q1002"); // ręczna

			Assert.Contains("Towar dołączono", result.Message);
			var handTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id);

			Assert.Equal(PickingStatus.PickedPartially, handTask.PickingStatus);
			Assert.Equal(20, handTask.RequestedQuantity);
			Assert.Equal(12, handTask.PickedQuantity);

			var virtualPallet = DbContext.VirtualPallets
			.Include(v => v.PickingTasks)
			.Single(v => v.PalletId == sourcePallet1.Id);

			Assert.NotNull(virtualPallet);
			Assert.Equal(sourcePallet1.Id, virtualPallet.PalletId);
			Assert.Single(virtualPallet.PickingTasks);

			var pickingTask = virtualPallet.PickingTasks.Single();

			Assert.Equal(issue.Id, pickingTask.IssueId);
			Assert.Equal(product1.Id, pickingTask.ProductId);
			Assert.Equal(20, pickingTask.RequestedQuantity);
			Assert.Equal(PickingStatus.PickedPartially, pickingTask.PickingStatus);
			Assert.Equal(sourcePallet1.Id, pickingTask.VirtualPallet.PalletId);

			var palletsAdded = DbContext.Pallets
				.Where(p => p.IssueId == issue.Id)
				.ToList();

			var sourcePalletAfter = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(p => p.PalletNumber == "Q1000");

			Assert.Equal(88, sourcePalletAfter.ProductsOnPallet.Single().Quantity);
			//
			var plannedTasks = DbContext.PickingTasks
				.Where(t => t.IssueId == issue.Id && t.PickingStatus == PickingStatus.Allocated)
				.ToList();

			Assert.Empty(plannedTasks);
			Assert.True(DbContext.HistoryPickings.Any(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id));

		}
		//
		[Fact]
		public async Task ExecutiveHandPicking_TaskPartialNeededNoVirtualPalletToMuchTake_AddToIssue()
		{
			// Arrange
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
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPickingZone = new Location
			{
				Id = 100100,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 10);
			//var handPicknigTask = new PickingTask
			//{
			//	IssueId = issue.Id,
			//	//CreateDate = DateTime.UtcNow.AddDays(-1),
			//	PickingStatus = PickingStatus.Available,
			//	RequestedQuantity = 20,
			//	PickedQuantity = 10,
			//	ProductId = product1.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(300)),
			//};
			DbContext.PickingTasks.Add(handPicknigTask);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 12, "UserCor", 100100));//12 to za dużo bo ma być 10
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Chcesz pobrać więcej niż potrzeba.", result.Error);
		}
	}
}
