using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Picking.Commands.ClosePickingPallet;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PickingPalletTests.Integration
{
	public class ClosePalletPickingIntegrationTests :TestBase
	{
		private static Client CreateClient()
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
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
		}
		private static Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
		}
		private static Product CreateProduct()
		{
			return Product.Create("Prod A", "666", TestDates.UtcNow, 1, 100, 30, 30, 30, 30, "TestDetails");
		}
		private static Location CreateLocation(int id, int aisle)
		{
			return new Location
			{
				Id = id,
				Aisle = aisle,
				Bay = 1,
				Height = 1,
				Position = 1
			};
		}
		[Fact]
		public async Task ClosePalletPicking_ShouldChangeStatus_WhenProperData()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct();
			var location1 = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 10);
			var client = CreateClient();
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));
			
			var pickingPallet = Pallet.CreateForTests("Q1001", TestDates.Now, 100100, PalletStatus.Picking, null, null);
			pickingPallet.AddProductForTests(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.Now.AddMonths(24)));
			
			var issueId = Guid.NewGuid();
			
			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.Now.AddDays(-6),
			DateOnly.FromDateTime( TestDates.Now.AddDays(1)), "user1", IssueStatus.Pending, null);
		
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet, pickingPallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 40, sourcePallet.LocationId, TestDates.UtcNow.AddDays(-8));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 30, PickingStatus.Picked, product.Id,
				 DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)), pickingPallet.Id, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(-1)), 30);
			
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ClosePickingPalletCommand(pickingPallet.Id, issue.Id, "UserPicker"));
			//Assert
			Assert.NotNull(result);
			var closedPallet = DbContext.Pallets.SingleOrDefault(p => p.Id == pickingPallet.Id);
			Assert.NotNull(closedPallet);
			Assert.Equal(PalletStatus.ToIssue, closedPallet.Status);
			Assert.Equal(issue.Id, closedPallet.IssueId);
			var history = DbContext.HistoryPallet.SingleOrDefault(p => p.PalletId == pickingPallet.Id);
			Assert.NotNull(history);
			Assert.Equal(ReasonForPallet.ToLoad, history.Reason);
			Assert.Contains("Pallet was closed and added to issue", result.Message);
		}
		[Fact]
		public async Task ClosePalletPicking_ShouldReturnErrorInfo_WhenPalletAlreadyAssigned()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct();
			var location1 = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 10);
			var client = CreateClient();
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var pickingPallet = Pallet.CreateForTests("Q1001", TestDates.Now, 100100, PalletStatus.ToIssue, null, null);
			pickingPallet.AddProductForTests(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.Now.AddMonths(24)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.Now.AddDays(-6),
			DateOnly.FromDateTime(TestDates.Now.AddDays(1)), "user1", IssueStatus.Pending, null);

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet, pickingPallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 40, sourcePallet.LocationId, TestDates.UtcNow.AddDays(-8));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 30, PickingStatus.Picked, product.Id,
				 DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)), pickingPallet.Id, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(-1)), 30);

			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var ex = await Assert.ThrowsAsync<AlreadyAssignedDomainException>(() => Mediator.Send(new ClosePickingPalletCommand(pickingPallet.Id, issue.Id, "UserPicker")));
			Assert.Contains("Pallet already assigned.", ex.Message);
		}
		[Fact]
		public async Task ClosePalletPicking_ShouldThrowInvalidPalletStatus_WhenPalletIsNotPicking()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct();
			var location = CreateLocation(1, 1);
			var client = CreateClient();
			var pickingPallet = Pallet.CreateForTests("Q1001", TestDates.Now, location.Id, PalletStatus.Available, null, null);
			pickingPallet.AddProductForTests(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.Now.AddMonths(24)));
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, TestDates.Now.AddDays(-6),
				DateOnly.FromDateTime(TestDates.Now.AddDays(1)), "user1", IssueStatus.Pending, null);

			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.Pallets.Add(pickingPallet);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			//Act&Assert
			var ex = await Assert.ThrowsAsync<InvalidPalletStatusDomainException>(() =>
				Mediator.Send(new ClosePickingPalletCommand(pickingPallet.Id, issue.Id, "UserPicker")));
			Assert.Equal(pickingPallet.Id, ex.PalletId);
			Assert.Equal(pickingPallet.PalletNumber, ex.PalletNumber);
		}
	}
}
