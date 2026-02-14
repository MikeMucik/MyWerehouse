using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.DeleteIssue;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueDeleteIntegrationServiceTests : TestBase 
	{
		public static class TestHelper
		{
			public static async Task<Issue> CreateIssueWithoutPalletsAsync(Infrastructure.WerehouseDbContext db, IssueStatus status = IssueStatus.New)
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
				var issue = new Issue
				{
					IssueStatus = status,
					Client = client,
					PerformedBy = "UserInit",
					Pallets = new List<Pallet>(),
					PickingTasks = new List<PickingTask>()
				};
				db.Clients.Add(client);
				db.Issues.Add(issue);
				await db.SaveChangesAsync();
				return issue;
			}
			public static async Task<(Issue issue, List<Pallet> pallets)>
				SetupBasicIssue(Infrastructure.WerehouseDbContext db, IssueStatus issueStatus, int qty)
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
				var category = new Category { Name = "Cat", IsDeleted = false };
				var product = new Product
				{
					Name = "TestFull",
					SKU = "123",
					AddedItemAd = new DateTime(2024, 1, 1),
					Category = category,
					IsDeleted = false,
					CartonsPerPallet = 10,
				};			

				var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.InTransit,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new() { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.ToPicking,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new() { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
				var issue = new Issue
				{
					Client = client,
					IssueDateTimeCreate = DateTime.UtcNow,
					IssueStatus = issueStatus,
					IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
					PerformedBy = "User1",
					Pallets = pallets,
					PickingTasks = new List<PickingTask>()
				};
				foreach (var p in pallets)
					p.Issue = issue;
				// Dodaj przykładową alokację
				issue.PickingTasks.Add(new PickingTask
				{
					Issue = issue,
					RequestedQuantity = qty,
					PickingStatus = PickingStatus.Allocated,
					VirtualPallet = new VirtualPallet
					{
						Pallet = pallets[1],
						InitialPalletQuantity = qty,
						Location = location,
					}
				});
				db.Issues.Add(issue);
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
			Assert.True(result.Success);
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
				qty: 12);
			var issueId = setup.issue.Id;
			// Act
			var result = await Mediator.Send( new DeleteIssueCommand(issueId, "UserX"));

			// Assert
			Assert.True(result.Success);
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
			var history = DbContext.HistoryPickings
				.Where(h => h.PickingTaskId == issue.PickingTasks.First().Id)
				.ToList();

			Assert.NotEmpty(history);
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
			Assert.False(result.Success);
			Assert.Contains("nie można anulować", result.Message);

			// Nic nie powinno zostać zmienione
			var issue = DbContext.Issues.First(i => i.Id == issueId);
			Assert.Equal(IssueStatus.ConfirmedToLoad, issue.IssueStatus);

			// Palety nienaruszone
			foreach (var p in issue.Pallets)
				Assert.NotEqual(PalletStatus.Available, p.Status);
		}

	}
}
