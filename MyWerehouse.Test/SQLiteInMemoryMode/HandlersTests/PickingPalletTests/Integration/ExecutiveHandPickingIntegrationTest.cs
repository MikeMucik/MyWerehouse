using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Picking.Commands.ExecuteHandPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PickingPalletTests.Integration
{
	public class ExecutiveHandPickingIntegrationTest : TestBase
	{
		private Client CreateClient()
		{
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
			return new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
		}
		private Category CreateCategory(string name)
		{
			return new Category
			{
				Name = name,
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, 1, 100);
		}
		private Location CreateLocation(int id, int position)
		{
			return new Location
			{
				Id = id,
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		[Fact]
		public async Task ExecutiveHandPicking_ShouldPicked_WhenNoVirtualPallet()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPickingZone = CreateLocation(100100, 5);			
			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
		
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet, pallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 0);
			
			DbContext.PickingTasks.Add(handPicknigTask);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet.Id, issue.Id, 20, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Weź nową paletę dla zlecenia. Towar:", result.Result.Message);

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
			.Single(v => v.PalletId == sourcePallet.Id);

			Assert.NotNull(virtualPallet);
			Assert.Equal(sourcePallet.Id, virtualPallet.PalletId);
			Assert.Single(virtualPallet.PickingTasks);

			var pickingTask = DbContext.PickingTasks.Single();
			Assert.Equal(issue.Id, pickingTask.IssueId);
			Assert.Equal(product1.Id, pickingTask.ProductId);
			Assert.Equal(20, pickingTask.RequestedQuantity);
			Assert.Equal(PickingStatus.Picked, pickingTask.PickingStatus);
			Assert.Equal(sourcePallet.Id, pickingTask.VirtualPallet.PalletId);

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
		public async Task ExecutiveHandPicking_ShouldPicked_WhenVirtualPalletExist()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPickingZone = CreateLocation(100100, 5);			
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
					
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 0);
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 100,sourcePallet1.LocationId, new DateTime(2025, 8, 12));
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.PickingTasks.Add(handPicknigTask);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 20, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Weź nową paletę dla zlecenia. Towar:", result.Result.Message);

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
		public async Task ExecutiveHandPicking_ShouldPickedPartial_WhenNoVirtualPallet()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPickingZone = CreateLocation(100100, 5);
			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
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
			
			DbContext.PickingTasks.Add(handPicknigTask);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 12, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Weź nową paletę dla zlecenia. Towar:", result.Result.Message);

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
			Assert.NotNull(pickingTask);
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
		[Fact]
		public async Task ExecutiveHandPicking_ReturnInfoError_WhenToManyTake()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");			
			var location1 = CreateLocation(1, 1);
			var locationPickingZone = CreateLocation(100100, 5);			
			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));
			
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(DateTime.Now.AddDays(300)), null, null, 10);
			
			DbContext.PickingTasks.Add(handPicknigTask);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 12, "UserCor", 100100));//12 to za dużo bo ma być 10
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Chcesz pobrać więcej niż potrzeba.", result.Error);
		}
	}
}
