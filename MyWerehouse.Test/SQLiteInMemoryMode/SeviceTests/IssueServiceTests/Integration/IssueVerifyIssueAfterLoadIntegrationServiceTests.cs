using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Invetories.InventoryExceptions;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueVerifyIssueAfterLoadIntegrationServiceTests : TestBase
	{
		[Fact]
		public async Task VerifyIssueAfterLoadingAsync_IsValid_UpdateDatabase()
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
			var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var product1 = Product.Create("Prod2", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>
			{
				IssueItem.CreateForSeed(1, issueId, product.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product1.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.IsShipped, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet1.AddProduct(product1.Id, 10, new DateOnly(2026, 1, 1));

			//Inventory
			//dla dwóch produktów
			var inventory = new Inventory
			{
				Product = product,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			var inventory1 = new Inventory
			{
				Product = product1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			DbContext.Inventories.AddRange(inventory1, inventory);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			//Act
			var result = await Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "UserTest"));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal("Załadunek zatwierdzony, zasoby uaktulanione.", result.Message);

			//sprawdzenie zmian w Issue
			var issueFromDb = await DbContext.Issues
				.Include(i => i.Pallets)
				.ThenInclude(p => p.ProductsOnPallet)
				.FirstAsync(i => i.Id == issue.Id);
			Assert.Equal(IssueStatus.Archived, issueFromDb.IssueStatus);
			Assert.Equal("UserTest", issueFromDb.PerformedBy);

			//sprawdzenie statusu palet
			Assert.All(issueFromDb.Pallets, p => Assert.Equal(PalletStatus.Archived, p.Status));

			//sprawdzenie ilości w Inventory (po odjęciu 10 i 10)
			var inventoryFromDb = await DbContext.Inventories.FirstAsync(i => i.ProductId == product.Id);
			var inventory1FromDb = await DbContext.Inventories.FirstAsync(i => i.ProductId == product1.Id);
			Assert.Equal(90, inventoryFromDb.Quantity);
			Assert.Equal(90, inventory1FromDb.Quantity);

			//Sprawdzenie zapisu history
			var issueHistory = await DbContext.HistoryIssues
				.Include(h => h.Details)
				.FirstAsync(i => i.IssueId == issue.Id);
			Assert.NotNull(issueHistory);
			var palletHistory = await DbContext.PalletMovements
				.Include(p => p.PalletMovementDetails)
				.FirstAsync(ph => ph.PalletId == pallet.Id);
			Assert.NotNull(palletHistory);
			var palletHistory1 = await DbContext.PalletMovements
				.Include(p => p.PalletMovementDetails)
				.FirstAsync(ph => ph.PalletId == pallet1.Id);
			Assert.NotNull(palletHistory1);
		}
		[Fact]
		public async Task VerifyIssueAfterLoadingAsync_InvalidInventory_UpdateDatabase()
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
			var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var product1 = Product.Create("Prod2", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.AddRange(product, product1);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>
			{
				IssueItem.CreateForSeed(1, issueId, product.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product1.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.IsShipped, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet1.AddProduct(product1.Id, 10, new DateOnly(2026, 1, 1));

			//Inventory
			//dla dwóch produktów
			var inventory = new Inventory
			{
				Product = product,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 5
			};
			var inventory1 = new Inventory
			{
				Product = product1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 5
			};

			DbContext.AddRange(inventory, inventory1);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			//Act&Assert
			//var result = await Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "UserTest"));
			var ex = await Assert.ThrowsAsync<DomainInventoryException>(() => Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "UserTest")));
			//
			//Assert.NotNull(result);
			//Assert.False(result.IsSuccess);
			Assert.Equal($"Product {product.Id} quantity below zero - prohibited condition", ex.Message);

		}
		[Fact]
		public async Task VerifyIssueAfterLoadingAsync_IssueNotFound_ReturnFail()
		{
			var receiptId9 = Guid.Parse("91111111-1111-1111-1111-111111111111");
			//Act
			var result = await Mediator.Send(new VerifyIssueAfterLoadingCommand(receiptId9, "userX"));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Equal($"Zamówienie nie zostało znalezione.", result.Error);
		}
		[Fact]
		public async Task VerifyIssueAfterLoadingAsync_NotAllPalletIsLoaded_ThrowException_ReturnFail()
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
			var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var product1 = Product.Create("Prod2", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>
			{
				IssueItem.CreateForSeed(1, issueId, product.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product1.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.InProgress, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet1.AddProduct(product1.Id, 10, new DateOnly(2026, 1, 1));

			//Inventory
			//dla dwóch produktów
			var inventory = new Inventory
			{
				Product = product,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			var inventory1 = new Inventory
			{
				Product = product1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};

			DbContext.AddRange(inventory, inventory1);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			//var result = await Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "userX"));
			var ex = await Assert.ThrowsAsync<NotEndedLoadingException>(() => Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "UserTest")));

			//Assert
			//Assert.NotNull(result);
			//Assert.False(result.IsSuccess);
			Assert.Equal($"Issue {issueId} has pallets not fully loaded.", ex.Message);
		}
		[Fact]
		public async Task VerifyIssueAfterLoadingAsync_IssueNotShipped_ThrowException_ReturnFail()
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
			var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var product1 = Product.Create("Prod2", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.AddRange(product, product1);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>
			{
				IssueItem.CreateForSeed(1, issueId, product.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product1.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.InProgress, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, null);
			pallet1.AddProduct(product1.Id, 10, new DateOnly(2026, 1, 1));

			//Inventory
			//dla dwóch produktów
			var inventory = new Inventory
			{
				Product = product,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			var inventory1 = new Inventory
			{
				Product = product1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};

			DbContext.AddRange(inventory, inventory1);
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			//var result = await Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "userX"));
			var ex =await Assert.ThrowsAsync<NotAllowedOperationException>(async ()=> await Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "userX")));
			//Assert.NotNull(result);
			//Assert.False(result.IsSuccess);
			Assert.Equal($"Operation forbidden for {issueId}, wrong status.", ex.Message);
		}
		[Fact]
		public async Task VerifyIssueAfterLoading_ShouldUpdateStatusAndHistory_HappyPath()
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
			var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var product1 = Product.Create("Prod2", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.AddRange(product, product1);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>
			{
				IssueItem.CreateForSeed(1, issueId, product.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product1.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.IsShipped, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet1.AddProduct(product1.Id, 10, new DateOnly(2026, 1, 1));

			var inventory = new Inventory
			{
				Product = product,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			var inventory1 = new Inventory
			{
				Product = product1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			DbContext.AddRange(inventory, inventory1);
			await DbContext.SaveChangesAsync();

			// Act
			var result = await Mediator.Send(new VerifyIssueAfterLoadingCommand(issue.Id, "user1"));
			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal("Załadunek zatwierdzony, zasoby uaktulanione.", result.Message);

			// Sprawdzenie zmian w bazie
			var updatedIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Archived, updatedIssue.IssueStatus);
			Assert.All(updatedIssue.Pallets, p => Assert.Equal(PalletStatus.Archived, p.Status));

			// Sprawdzenie historii Issue
			var issueHistory = await DbContext.HistoryIssues
				.Include(h => h.Details)
				.FirstOrDefaultAsync(h => h.IssueId == issue.Id);
			Assert.NotNull(issueHistory);

			// Sprawdzenie historii palet
			var palletHistory = await DbContext.PalletMovements
				.Include(h => h.PalletMovementDetails)
				.FirstOrDefaultAsync(h => h.PalletId == pallet.Id);
			Assert.NotNull(palletHistory);
			Assert.Equal(PalletStatus.Archived, palletHistory.PalletStatus);
		}
	}
}
