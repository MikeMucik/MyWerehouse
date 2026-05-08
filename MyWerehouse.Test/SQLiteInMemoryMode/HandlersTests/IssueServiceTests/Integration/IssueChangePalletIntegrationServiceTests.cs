using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueChangePalletIntegrationServiceTests : TestBase
	{
		[Fact]
		public async Task ChangePalletDuringLoadingAsync_IsInvalid_UpdateIssue()
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
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 2, client.Id, DateTime.UtcNow.AddDays(-7),
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));			
			
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			
			// Act
			var result = await Mediator.Send(new ChangePalletInIssueCommand(issue.Id, pallet.Id, pallet1.Id, "tester"));

			// Assert
			Assert.True(result.Result.Success);
			Assert.Equal("Podmieniono palety.", result.Result.Message);

			var updatedIssue = await DbContext.Issues.Include(i => i.Pallets).FirstAsync(i => i.Id == issue.Id);
			var p1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			var p2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");

			Assert.DoesNotContain(updatedIssue.Pallets, p => p.PalletNumber == "P1");
			Assert.Contains(updatedIssue.Pallets, p => p.PalletNumber == "P2");

			Assert.Equal(PalletStatus.Available, p1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, p2.Status);
			Assert.Equal(IssueStatus.ChangingPallet, updatedIssue.IssueStatus);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenSamePalletIds()
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 2, client.Id, DateTime.UtcNow.AddDays(-7),
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));			
			
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//&Act
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var result = await Mediator.Send(new ChangePalletInIssueCommand(receiptId1, pallet.Id, pallet.Id, "tester"));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("tą samą", result.Error);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenIssueNotFound()
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, null);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			DbContext.Pallets.AddRange(pallet, pallet1);
			//DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//pallet.MoveToLocation(location, "user");
			// Act
			var receiptId9 = Guid.Parse("91111111-1111-1111-1111-111111111111");
			var result = await Mediator.Send(new ChangePalletInIssueCommand(receiptId9, pallet.Id, pallet1.Id, "tester"));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("Zamówienie nie zostało znalezione.", result.Error);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenDifferentProducts()
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var productA = Product.Create("ProdA", "A", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, productA);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 2, client.Id, DateTime.UtcNow.AddDays(-7),
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(productA.Id, 10, new DateOnly(2026, 1, 1));			
			
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await Mediator.Send(new ChangePalletInIssueCommand(issue.Id, pallet.Id, pallet1.Id, "tester"));

			// Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("różnymi produktami", result.Error);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenNewPalletHasInvalidStatus()
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, DateTime.UtcNow.AddDays(-7),
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, location.Id, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, location.Id, PalletStatus.ToIssue, null, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));		
			
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ChangePalletInIssueCommand(issue.Id, pallet.Id, pallet1.Id, "tester"));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("błędny status", result.Error);
		}
	}
}

