using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using Xunit.Sdk;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.IssueTests.Integration
{
	public class IssueCompletedTests : TestBase
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
		private Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "Cat",
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name)
		{
			return Product.Create(name, "SKU1", 1, 10);
		}
		private Location CreateLocation(int position)
		{
			return new Location
			{
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		[Fact]
		public async Task CompletedLoadIssue_ShouldChangeStatusIssue_WhenAllLoaded()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory();
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1");			
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
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.ConfirmedToLoad, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));


			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			//Act
			var result = await Mediator.Send(new CompletedLoadIssueCommand(issue.Id, "UserLoader"));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
		}

		[Fact]
		public async Task CompletedLoadIssue_ShouldReturnError_WhenIssueNotExist()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory();
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1");

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
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.ConfirmedToLoad, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			//Act
			DbContext.Issues.Remove(issue);
			DbContext.SaveChanges();
			var result = await Mediator.Send(new CompletedLoadIssueCommand(issue.Id, "UserLoader"));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.NotEqual(IssueStatus.IsShipped, issue.IssueStatus);
		}
		[Fact]
		public async Task CompletedLoadIssue_ShouldReturnError_WhenNotAllPalletLoaded()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory();
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1");

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

			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Loaded, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act&Assert
			var ex = await Assert.ThrowsAsync<NotEndedLoadingDomainException>(() => Mediator.Send(new CompletedLoadIssueCommand(issue.Id, "UserLoader")));
			Assert.Equal($"Issue {issueId} has pallets not fully loaded.", ex.Message);
		}
	}
}
