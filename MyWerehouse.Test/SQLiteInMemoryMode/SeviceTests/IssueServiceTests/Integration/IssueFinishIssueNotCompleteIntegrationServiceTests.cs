using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueFinishIssueNotCompleteIntegrationServiceTests : TestBase
	{
		[Fact]
		public async Task FinishIssueNotCompleted_ShouldUpdateStatusesAndCreateMovementsAndHistory()
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
			var cLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var location1 = new Location
			{
				Id = 10,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Id = 20,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product1 = Product.Create("Test", "666666", 1, 56);
			
			var product2 = Product.Create("Test1", "666666", 1, 65);

			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location1, location2);
			DbContext.Clients.Add(cLient);
			DbContext.SaveChanges();
			var issueId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var issueId = issueId1;
			var issueItem = new List<IssueItem>
			{
				IssueItem.CreateForSeed(1, issueId, product1.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product2.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var performedBy = "Janek";
			var loadedPallet = Pallet.CreateForTests("P1", DateTime.UtcNow, location1.Id, PalletStatus.Loaded, null, issueId);
			loadedPallet.AddProduct(product1.Id, 5, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var notLoadedPallet = Pallet.CreateForTests("P2", DateTime.UtcNow, location2.Id, PalletStatus.ToIssue, null,issueId);
			notLoadedPallet.AddProduct(product2.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var issue = Issue.CreateForSeed(issueId, 2, cLient.Id, new DateTime(2025, 6, 6, 2, 2, 2),
			new DateTime(2025, 6, 12, 2, 2, 2), "TestUser", IssueStatus.Pending, issueItem);
						
			DbContext.Pallets.AddRange(notLoadedPallet, loadedPallet);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act			
			await Mediator.Send(new FinishIssueNotCompletedCommand(issueId, performedBy));
			// Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(PalletStatus.Available, notLoadedPallet.Status);
			Assert.Null(notLoadedPallet.IssueId);
			var updatedIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstOrDefaultAsync(i => i.Id == issueId);

			Assert.NotNull(updatedIssue);
			Assert.Equal(IssueStatus.IsShipped, updatedIssue.IssueStatus);

			// sprawdź czy P2 została usunięta z przypisania do zlecenia:
			var palletP2 = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "P2");
			Assert.NotNull(palletP2);
			Assert.Equal(PalletStatus.Available, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			// Historia palet — sprawdź, czy została utworzona dla załadowanej palety
			var palletHistories = await DbContext.PalletMovements
				.FirstOrDefaultAsync(h => h.PalletNumber == "P1");

			Assert.NotNull(palletHistories);
			Assert.Equal(PalletStatus.Loaded, palletHistories.PalletStatus);
			Assert.Equal(performedBy, palletHistories.PerformedBy);

			// Sprawdź, że dla palety P2 (niezaładowanej) też utworzono historię zmiany statusu
			var palletHistoryP2 = await DbContext.PalletMovements
				.FirstOrDefaultAsync(h => h.PalletNumber == "P2");
			Assert.NotNull(palletHistoryP2);
			Assert.Equal(PalletStatus.Available, palletHistoryP2.PalletStatus);
			Assert.Equal(performedBy, palletHistoryP2.PerformedBy);

			// Historia załadunku (np. HistoryLoading) — sprawdź, że powstał wpis dla zlecenia
			var loadingHistories = await DbContext.HistoryIssues
				.FirstOrDefaultAsync(h => h.IssueId == issueId);
			Assert.NotNull(loadingHistories);

			// Sprawdź, że zawiera wpis dla załadowanej palety P1
			Assert.Contains(loadingHistories.Details, h => h.PalletNumber == "P1");

			// Sprawdź, że status i wykonawca się zgadzają			
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(performedBy, updatedIssue.PerformedBy);
		}
	}
}
