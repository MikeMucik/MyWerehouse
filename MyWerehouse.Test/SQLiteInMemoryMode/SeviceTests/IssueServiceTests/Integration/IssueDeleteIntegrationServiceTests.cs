using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.DeleteIssue;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueDeleteIntegrationServiceTests : TestBase
	{
		public static class TestHelper
		{
			public static async Task<Issue> CreateIssueWithoutPalletsAsync(WerehouseDbContext db, IssueStatus status = IssueStatus.New)
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
				var client = new Client
				{
					Name = "TestCompany",
					Email = "123@op.pl",
					Description = "Description",
					FullName = "FullNameCompany",
					Addresses = new List<Address> { address }
				};

				var issueId = Guid.NewGuid();

				var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow.AddDays(-7),
				DateTime.UtcNow.AddDays(7), "UserInit", status, null);

				db.Clients.Add(client);
				db.Issues.Add(issue);
				await db.SaveChangesAsync();
				return issue;
			}
			public static async Task<(Issue issue, List<Pallet> pallets)>
				SetupBasicIssue(WerehouseDbContext db, IssueStatus issueStatus, int qty)
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
				var client = new Client
				{
					Name = "TestCompany",
					Email = "123@op.pl",
					Description = "Description",
					FullName = "FullNameCompany",
					Addresses = new List<Address> { address }
				};
				var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
				var category = new Category { Id = 1, Name = "Cat", IsDeleted = false };
				var product = Product.Create("TestFull", "123", 1, 10);
				db.Clients.Add(client);
				db.Categories.Add(category);
				db.Products.Add(product);
				db.Locations.Add(location);
				db.SaveChanges();
				var pallets = new List<Pallet>();
				var issueId = Guid.NewGuid();
				var issueItem = new List<IssueItem> { IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)) };
				var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow.AddDays(-7),
				DateTime.UtcNow.AddDays(7), "UserInit", issueStatus, issueItem);
				var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.InTransit, null, issue.Id);
				pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
				pallets.Add(pallet1);
				var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToPicking, null, issue.Id);
				pallet2.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
				pallets.Add(pallet2);
				// Dodaj przykładową alokację
				var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet2.Id, 10,pallet2.LocationId, new DateTime(2025, 8, 12));
				var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, qty, PickingStatus.Allocated, product.Id,
					null, null, null, 0);
				db.Issues.Add(issue);
				db.Pallets.AddRange(pallet1, pallet2);
				//await db.SaveChangesAsync();
				db.VirtualPallets.Add(virtualPallet);
				db.PickingTasks.Add(pickingTask);
				await db.SaveChangesAsync();
				return (issue, pallets);
			}
		}
		[Fact]
		public async Task DeleteIssueAsync_StatusNew_DeletesIssue()
		{
			// Arrange
			var issue = await TestHelper.CreateIssueWithoutPalletsAsync(DbContext, IssueStatus.New);
			var issueId = issue.Id;
			// Act
			var result = await Mediator.Send(new DeleteIssueCommand(issueId, "UserX"));
			// Assert
			Assert.True(result.IsSuccess);
			var issueExists = DbContext.Issues.Any(i => i.Id == issueId);
			Assert.False(issueExists); // fizycznie usunięte
									   // Brak palet lub alokacji związanych z tym issue
			Assert.Empty(DbContext.Pallets.Where(p => p.IssueId == issueId));
			Assert.Empty(DbContext.PickingTasks.Where(a => a.IssueId == issueId));
		}
		[Theory]
		[InlineData(IssueStatus.Pending)]
		//[InlineData(IssueStatus.NotComplete)]
		public async Task DeleteIssueAsync_StatusPendingOrNotComplete_CancelsIssueAndResetsPalletsAndPickingTasks(IssueStatus status)
		{
			// Arrange
			var setup = await TestHelper.SetupBasicIssue(
				DbContext,
				issueStatus: status,
				qty: 10);//12
			var issueId = setup.issue.Id;
			// Act
			var result = await Mediator.Send(new DeleteIssueCommand(issueId, "UserX"));

			// Assert
			Assert.True(result.IsSuccess);
			var issue = DbContext.Issues
				.Include(i => i.Pallets)
				.Include(i => i.PickingTasks)
				.First(i => i.Id == issueId);
			Assert.Equal(IssueStatus.Cancelled, issue.IssueStatus);
			// Palety uwolnione
			foreach (var p in issue.Pallets)
			{
				Assert.Equal(PalletStatus.Available, p.Status);
				Assert.Null(p.IssueId);
			}
			// Alokacje wyzerowane i anulowane
			foreach (var a in issue.PickingTasks)
			{
				Assert.Equal(0, a.RequestedQuantity);
				Assert.Equal(PickingStatus.Cancelled, a.PickingStatus);
			}
			// Historia powinna się dodać (jeśli masz tabelę HistoryPicking)
			var hhistory = DbContext.PickingTasks.FirstOrDefault(h => h.IssueId == issueId);
			var history = DbContext.HistoryPickings
				.Where(h => h.PickingTaskId == issue.PickingTasks.First().Id)
				.ToList();

			Assert.NotEmpty(history);
			//Assert.NotNull(hhistory);
		}
		[Fact]
		public async Task DeleteIssueAsync_StatusOther_ReturnsFail()
		{
			// Arrange
			var setup = await TestHelper.SetupBasicIssue(
				DbContext,
				issueStatus: IssueStatus.ConfirmedToLoad,
				qty: 10);

			var issueId = setup.issue.Id;

			// Act
			var result = await Mediator.Send(new DeleteIssueCommand(issueId, "UserX"));

			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("nie można anulować", result.Error);

			// Nic nie powinno zostać zmienione
			var issue = DbContext.Issues.First(i => i.Id == issueId);
			Assert.Equal(IssueStatus.ConfirmedToLoad, issue.IssueStatus);

			// Palety nienaruszone
			foreach (var p in issue.Pallets)
				Assert.NotEqual(PalletStatus.Available, p.Status);
		}

	}
}
