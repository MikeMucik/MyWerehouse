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
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Picking.PickingExceptions;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
			return Product.Create(name, sku, TestDates.UtcNow, 1, 100, 30, 30, 30, 30, "TestDetails");
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
			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet, pallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(TestDates.Now.AddDays(300)), null, null, 0);

			DbContext.PickingTasks.Add(handPicknigTask);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet.Id, issue.Id, 20, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Take a new pallet for the issue. Product:", result.Result.Message);

			var pallets = DbContext.Pallets.Where(p => p.IssueId == issue.Id).ToList();
			Assert.Equal(2, pallets.Count);
			Assert.Contains(pallets, p => p.PalletNumber == "Q1001"); // pierwotna
			Assert.Contains(pallets, p => p.PalletNumber == "Q1002"); // ręczna

			Assert.Contains("Product was added to the issue.", result.Message);
			var handTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id && h.VirtualPalletId == null);
			var executedTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id && h.VirtualPalletId != null);

			Assert.Equal(PickingStatus.Picked, executedTask.PickingStatus);
			//Assert.Equal(20, handTask.RequestedQuantity);

			var virtualPallet = DbContext.VirtualPallets
			.Include(v => v.PickingTasks)
			.Single(v => v.PalletId == sourcePallet.Id);

			Assert.NotNull(virtualPallet);
			Assert.Equal(sourcePallet.Id, virtualPallet.PalletId);
			Assert.Single(virtualPallet.PickingTasks);

			//var pickingTask = DbContext.PickingTasks.Single();
			Assert.NotNull(executedTask.VirtualPallet);
			Assert.Equal(issue.Id, executedTask.IssueId);
			Assert.Equal(product1.Id, executedTask.ProductId);
			//Assert.Equal(20, pickingTask.RequestedQuantity);
			Assert.Equal(0, handTask.RequestedQuantity);
			Assert.Equal(PickingStatus.Picked, executedTask.PickingStatus);
			Assert.Equal(sourcePallet.Id, executedTask.VirtualPallet.PalletId);

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

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(TestDates.Now.AddDays(300)), null, null, 0);
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 100, sourcePallet1.LocationId, new DateTime(2025, 8, 12));
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
			Assert.Contains("Take a new pallet for the issue. Product:", result.Result.Message);

			var pallets = DbContext.Pallets.Where(p => p.IssueId == issue.Id).ToList();
			Assert.Equal(2, pallets.Count);
			Assert.Contains(pallets, p => p.PalletNumber == "Q1001"); // pierwotna
			Assert.Contains(pallets, p => p.PalletNumber == "Q1002"); // ręczna

			Assert.Contains("Product was added to the issue.", result.Message);
			var handTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id && h.VirtualPalletId == null);
			var executedTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id && h.VirtualPalletId != null);

			Assert.Equal(PickingStatus.Picked, executedTask.PickingStatus);
			Assert.Equal(0, handTask.RequestedQuantity);

			var virtualPalletOld = DbContext.VirtualPallets
			.Include(v => v.PickingTasks)
			.Single(v => v.PalletId == sourcePallet1.Id);

			Assert.NotNull(virtualPalletOld);
			Assert.Equal(sourcePallet1.Id, virtualPalletOld.PalletId);
			Assert.Single(virtualPalletOld.PickingTasks);

			Assert.NotNull(executedTask.VirtualPallet);
			Assert.Equal(issue.Id, executedTask.IssueId);
			Assert.Equal(product1.Id, executedTask.ProductId);
			Assert.Equal(20, executedTask.RequestedQuantity);
			Assert.Equal(20, executedTask.PickedQuantity);
			Assert.Equal(PickingStatus.Picked, executedTask.PickingStatus);
			Assert.Equal(sourcePallet1.Id, executedTask.VirtualPallet.PalletId);

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

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 20, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(TestDates.Now.AddDays(300)), null, null, 0);

			DbContext.PickingTasks.Add(handPicknigTask);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 12, "UserCor", 100100));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Take a new pallet for the issue. Product:", result.Result.Message);

			var pallets = DbContext.Pallets.Where(p => p.IssueId == issue.Id).ToList();
			Assert.Equal(2, pallets.Count);
			Assert.Contains(pallets, p => p.PalletNumber == "Q1001"); // pierwotna
			Assert.Contains(pallets, p => p.PalletNumber == "Q1002"); // ręczna

			Assert.Contains("Product was added to the issue.", result.Message);
			var handTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id && h.VirtualPalletId == null);
			var executedTask = DbContext.PickingTasks.Single(h =>
				h.IssueId == issue.Id &&
				h.ProductId == product1.Id && h.VirtualPalletId != null);

			Assert.Equal(PickingStatus.Available, handTask.PickingStatus);
			Assert.Equal(8, handTask.RequestedQuantity);
			Assert.Equal(0, handTask.PickedQuantity);

			var virtualPallet = DbContext.VirtualPallets
			.Include(v => v.PickingTasks)
			.Single(v => v.PalletId == sourcePallet1.Id);

			Assert.NotNull(virtualPallet);
			Assert.Equal(sourcePallet1.Id, virtualPallet.PalletId);
			Assert.Single(virtualPallet.PickingTasks);

			Assert.NotNull(executedTask);
			Assert.NotNull(executedTask.VirtualPallet);
			Assert.Equal(issue.Id, executedTask.IssueId);
			Assert.Equal(product1.Id, executedTask.ProductId);
			Assert.Equal(12, executedTask.RequestedQuantity);
			Assert.Equal(12, executedTask.PickedQuantity);
			Assert.Equal(PickingStatus.Picked, executedTask.PickingStatus);
			Assert.Equal(sourcePallet1.Id, executedTask.VirtualPallet.PalletId);

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
			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet1.AddProductForTests(product1.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			var pallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPickingZone);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1);
			DbContext.Pallets.AddRange(sourcePallet1, pallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var pickingGuid = Guid.NewGuid();
			var handPicknigTask = PickingTask.CreateForSeed(pickingGuid, null, issue.Id, 12, PickingStatus.Available, product1.Id,
			 DateOnly.FromDateTime(TestDates.Now.AddDays(300)), null, null, 10);

			DbContext.PickingTasks.Add(handPicknigTask);
			DbContext.SaveChanges();
			//Act&Arrange
			var ex = await Assert.ThrowsAsync<TooHighValueDomainException>(()
				=> Mediator.Send(new ExecuteHandPickingCommand(sourcePallet1.Id, issue.Id, 20, "UserCor", 100100)));
			Assert.Contains($"Cannot pick 20 more than requested quantity {handPicknigTask.RequestedQuantity}.", ex.Message);
		}
		[Fact]
		public async Task ExecutiveHandPicking_ShouldCompletePickingFromTwoPallets_WhenFirstPalletHasInsufficientStock()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Prod A", "666");
			var location1 = CreateLocation(1, 1);
			var location2 = CreateLocation(2, 2);
			var locationPickingZone = CreateLocation(100100, 5);
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

			var firstSourcePallet = Pallet.CreateForTests(
				"Q1000", new DateTime(2025, 8, 8), location1.Id, PalletStatus.Available, null, null);
			firstSourcePallet.AddProductForTests(
				product.Id, 12, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			var secondSourcePallet = Pallet.CreateForTests(
				"Q1001", new DateTime(2025, 8, 8), location2.Id, PalletStatus.Available, null, null);
			secondSourcePallet.AddProductForTests(
				product.Id, 8, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)));

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, locationPickingZone);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.Pallets.AddRange(firstSourcePallet, secondSourcePallet);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			var handPickingTask = PickingTask.CreateForSeed(
				Guid.NewGuid(), null, issue.Id, 20, PickingStatus.Available, product.Id,
				DateOnly.FromDateTime(TestDates.Now.AddDays(300)), null, null, 0);
			DbContext.PickingTasks.Add(handPickingTask);
			await DbContext.SaveChangesAsync();

			// Act
			var firstResult = await Mediator.Send(
				new ExecuteHandPickingCommand(firstSourcePallet.Id, issue.Id, 12, "UserCor", 100100));
			var secondResult = await Mediator.Send(
				new ExecuteHandPickingCommand(secondSourcePallet.Id, issue.Id, 8, "UserCor", 100100));

			// Assert
			Assert.True(firstResult.IsSuccess);
			Assert.NotNull(firstResult.Result);
			Assert.True(firstResult.Result.NewPalletCreated);
			Assert.Contains("Take a new pallet for the issue. Product:", firstResult.Result.Message);

			Assert.True(secondResult.IsSuccess);
			Assert.NotNull(secondResult.Result);
			Assert.False(secondResult.Result.NewPalletCreated);
			Assert.Contains("Add the product to the existing picking pallet. Product:", secondResult.Result.Message);

			var handTask = DbContext.PickingTasks.Single(t =>
				t.Id == handPickingTask.Id &&
				t.VirtualPalletId == null);
			Assert.Equal(0, handTask.RequestedQuantity);
			Assert.Equal(0, handTask.PickedQuantity);
			Assert.Equal(PickingStatus.Cancelled, handTask.PickingStatus);

			var executedTasks = DbContext.PickingTasks
				.Include(t => t.VirtualPallet)
				.Where(t =>
					t.IssueId == issue.Id &&
					t.ProductId == product.Id &&
					t.VirtualPalletId != null)
				.ToList();

			Assert.Equal(2, executedTasks.Count);
			Assert.All(executedTasks, task => Assert.Equal(PickingStatus.Picked, task.PickingStatus));
			Assert.All(executedTasks, task => Assert.Equal(task.RequestedQuantity, task.PickedQuantity));

			var firstExecutedTask = executedTasks.Single(t => t.VirtualPallet!.PalletId == firstSourcePallet.Id);
			Assert.Equal(12, firstExecutedTask.RequestedQuantity);

			var secondExecutedTask = executedTasks.Single(t => t.VirtualPallet!.PalletId == secondSourcePallet.Id);
			Assert.Equal(8, secondExecutedTask.RequestedQuantity);

			var virtualPallets = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.Where(v => v.PalletId == firstSourcePallet.Id || v.PalletId == secondSourcePallet.Id)
				.ToList();
			Assert.Equal(2, virtualPallets.Count);
			Assert.All(virtualPallets, virtualPallet => Assert.Single(virtualPallet.PickingTasks));

			var pickingPallet = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(p => p.IssueId == issue.Id && p.Status == PalletStatus.Picking);
			Assert.Equal(20, pickingPallet.ProductsOnPallet.Single().Quantity);

			var sourcePalletsAfterPicking = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p => p.Id == firstSourcePallet.Id || p.Id == secondSourcePallet.Id)
				.ToList();
			Assert.All(sourcePalletsAfterPicking, pallet =>
			{
				Assert.Equal(PalletStatus.Archived, pallet.Status);
				Assert.Equal(0, pallet.ProductsOnPallet.Single().Quantity);
			});

			var allocatedTasks = DbContext.PickingTasks
				.Where(t => t.IssueId == issue.Id && t.PickingStatus == PickingStatus.Allocated)
				.ToList();
			Assert.Empty(allocatedTasks);
		}

		[Fact]
		public async Task ExecutiveHandPicking_ShouldThrow_WhenSourcePalletHasDifferentBestBefore()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Prod A", "666");
			var location = CreateLocation(1, 1);
			var locationPickingZone = CreateLocation(100100, 5);
			var expectedBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365));
			var sourceBestBefore = expectedBestBefore.AddDays(-30);

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPickingZone);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			await DbContext.SaveChangesAsync();

			var issue = Issue.CreateForSeed(
				Guid.NewGuid(),
				1,
				client.Id,
				TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)),
				"TestUser",
				IssueStatus.Pending,
				null);

			var sourcePallet = Pallet.CreateForTests(
				"Q1000",
				TestDates.UtcNow,
				location.Id,
				PalletStatus.Available,
				null,
				null);
			sourcePallet.AddProductForTests(
				product.Id,
				20,
				TestDates.UtcNow,
				sourceBestBefore);

			DbContext.Pallets.Add(sourcePallet);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			var handPickingTask = PickingTask.CreateForSeed(
				Guid.NewGuid(),
				null,
				issue.Id,
				20,
				PickingStatus.Available,
				product.Id,
				expectedBestBefore,
				null,
				null,
				0);
			DbContext.PickingTasks.Add(handPickingTask);
			await DbContext.SaveChangesAsync();

			// Act
			await Assert.ThrowsAsync<NotCorrectDateBestBeforeDomainException>(() =>
				Mediator.Send(new ExecuteHandPickingCommand(
					sourcePallet.Id,
					issue.Id,
					10,
					"UserCor",
					locationPickingZone.Id)));

			// Assert
			await using var freshContext = CreateNewContext();
			var sourcePalletAfter = await freshContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.SingleAsync(p => p.Id == sourcePallet.Id);
			var handTaskAfter = await freshContext.PickingTasks
				.SingleAsync(t => t.Id == handPickingTask.Id);

			Assert.Equal(PalletStatus.Available, sourcePalletAfter.Status);
			Assert.Equal(20, sourcePalletAfter.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PickingStatus.Available, handTaskAfter.PickingStatus);
			Assert.Equal(20, handTaskAfter.RequestedQuantity);
			Assert.Equal(0, handTaskAfter.PickedQuantity);
			Assert.False(await freshContext.VirtualPallets.AnyAsync(v => v.PalletId == sourcePallet.Id));
			Assert.False(await freshContext.Pallets.AnyAsync(p =>
				p.IssueId == issue.Id &&
				p.Status == PalletStatus.Picking));
			Assert.Equal(1, await freshContext.PickingTasks.CountAsync(t => t.IssueId == issue.Id));
		}
	}
}
