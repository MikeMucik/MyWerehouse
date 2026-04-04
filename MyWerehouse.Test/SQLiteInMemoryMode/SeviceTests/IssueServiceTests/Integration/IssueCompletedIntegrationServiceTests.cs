using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueCompletedIntegrationServiceTests : TestBase
	{
		[Fact]
		public async Task CompletedIssueAsync_AllLoaded_HappyPath()
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
		public async Task CompletedIssueAsync_AllLoaded_SadPath()
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
			//Act
			var result = await Mediator.Send(new CompletedLoadIssueCommand(issue.Id, "UserLoader"));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.NotEqual(IssueStatus.IsShipped, issue.IssueStatus);
		}
	}
}
