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
			var product1 = new Product
			{
				//Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var product2 = new Product
			{
				//Id = 11,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var issueId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var issueId = issueId1;
			var performedBy = "Janek";
			var loadedPallet = new Pallet
			{
				PalletNumber = "P1",
				Status = PalletStatus.Loaded,
				LocationId = 10,
				ProductsOnPallet = new List<ProductOnPallet>
		{
			new ProductOnPallet { ProductId = product1.Id, Quantity = 5, }
		}
			};
			var notLoadedPallet = new Pallet
			{
				PalletNumber = "P2",
				Status = PalletStatus.ToIssue,
				LocationId = 20,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet{ProductId =product2.Id, Quantity =10 } }
			};
			var issue = new Issue
			{
				Id = issueId,
				IssueNumber = 1,
				ClientId = cLient.Id,
				IssueDateTimeCreate = new DateTime(2025, 6, 6, 2, 2, 2),
				Pallets = new List<Pallet> { loadedPallet, notLoadedPallet },
				PerformedBy = "TestUser",
			};
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var inventory1 = new Inventory
			{
				Product = product1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			var inventory2 = new Inventory
			{
				Product = product2,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			DbContext.Inventories.AddRange(inventory1, inventory2);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location1, location2);
			DbContext.Clients.Add(cLient);
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
			//var palletP2 = await DbContext.Pallets.FindAsync("P2");
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
			//Inventory - nie sprawdzamy po
			//sprawdzenie ilości w Inventory (po odjęciu 10 i 0)
			//var inventoryFromDb = await DbContext.Inventories.FirstAsync(i => i.ProductId == product1.Id);
			//var inventory1FromDb = await DbContext.Inventories.FirstAsync(i => i.ProductId == product2.Id);
			//Assert.Equal(95, inventoryFromDb.Quantity);
			//Assert.Equal(100, inventory1FromDb.Quantity);

		}
	}
}
