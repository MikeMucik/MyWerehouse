using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueVerifyIssueToLoadIntegrationServiceTests : TestBase
	{
		[Fact]
		public async Task VerifyIssueToLoadAsync_IsValid_ConfirmIssue()
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
			var category = new Category {Id =1,  Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 56);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
						
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			await Mediator.Send(new VerifyIssueToLoadCommand(issue.Id, "user123"));
			//Assert
			var receipt = DbContext.Issues.Find(issue.Id);
			Assert.NotNull(receipt);
			Assert.Equal(IssueStatus.ConfirmedToLoad, receipt.IssueStatus);
			var savedIssue = await DbContext.Issues
				.Include(i => i.Pallets).ThenInclude(p => p.ProductsOnPallet)
				.Include(i => i.IssueItems)
				.FirstOrDefaultAsync(i => i.Id == issue.Id);
			Assert.NotNull(savedIssue);
			Assert.Equal(2, savedIssue.Pallets.Count);
			Assert.Single(savedIssue.IssueItems);
			Assert.All(savedIssue.Pallets, p => Assert.True(p.ProductsOnPallet.Any()));
		}
		[Fact]
		public async Task VerifyIssueToLoadAsync_IsInvalid_NotConfirmIssue()
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
			var category = new Category {Id =1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 56);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateTime.Now.AddDays(1), "user1", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			var receiptId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			var result = await Mediator.Send(new VerifyIssueToLoadCommand(receiptId2, "user123"));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
		}
	}
}
