using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.IssueTests.Integration
{
	public class IssueVerifyIssueToLoadTests : TestBase
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
		private Category CreateCategory(string name)
		{
			return new Category
			{
				//Id = 1,
				Name = name,
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name, string sku, int categoryId)
		{
			return Product.Create(name, sku, categoryId, 10);
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
		public async Task VerifyIssueToLoadAsync_ShouldChnageStatus_WhenStatusPending()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1", "SKU1", 1);

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
			DateOnly.FromDateTime(DateTime.Now.AddDays(1)), "user1", IssueStatus.Pending, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new VerifyIssueToLoadCommand(issue.Id, "user123"));
			//Assert
			Assert.True(result.IsSuccess);
			var issueAfter = DbContext.Issues.Find(issue.Id);
			Assert.NotNull(issueAfter);
			Assert.Equal(IssueStatus.ConfirmedToLoad, issueAfter.IssueStatus);
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
		public async Task VerifyIssueToLoadAsync_ShouldChnageStatus_WhenStatusInProgress()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1", "SKU1", 1);

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
			DateOnly.FromDateTime(DateTime.Now.AddDays(1)), "user1", IssueStatus.InProgress, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new VerifyIssueToLoadCommand(issue.Id, "user123"));
			//Assert
			Assert.True(result.IsSuccess);
			var issueAfter = DbContext.Issues.Find(issue.Id);
			Assert.NotNull(issueAfter);
			Assert.Equal(IssueStatus.ConfirmedToLoad, issueAfter.IssueStatus);
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
		public async Task VerifyIssueToLoadAsync_ShouldChnageStatus_WhenWrongStatusPallets()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1", "SKU1", 1);

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
			DateOnly.FromDateTime(DateTime.Now.AddDays(1)), "user1", IssueStatus.InProgress, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.LockedForIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new VerifyIssueToLoadCommand(issue.Id, "user123"));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("Wydania nie zatwierdzono.", result.Error);
			Assert.Contains(result.Result, x => x.Message.Contains("Nie wszystkie palety do załadunku mają odpowiedni status."));

		}
		[Fact]
		public async Task VerifyIssueToLoadAsync_ShouldChnageStatus_WhenWrondBBDate()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var location = CreateLocation(1);
			var product = CreateProduct("Prod1", "SKU1", 1);

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2027, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-7),
			DateOnly.FromDateTime(DateTime.Now.AddDays(1)), "user1", IssueStatus.InProgress, issueItem);
			var pallet = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet1 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, issueId);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new VerifyIssueToLoadCommand(issue.Id, "user123"));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("Wydania nie zatwierdzono.", result.Error);
			Assert.Equal(ErrorType.Validation, result.ErrorType);
			//Assert.Contains(result.Result, x => x.Message.Contains("Towar się nie zgadza. Zażądano 20 a przygotowano 0."));

			var failedItem = Assert.Single(result.Result.Where(x => !x.Success));
			var expectedQuantityRequest = issueItem.FirstOrDefault(p => p.ProductId == product.Id).Quantity;
			var expectedQuantityPrepared = 0; // Date BestBefore not equal
				//pallet.ProductsOnPallet.FirstOrDefault(p => p.ProductId == product.Id).Quantity + pallet1.ProductsOnPallet.FirstOrDefault(p => p.ProductId == product.Id).Quantity;
			Assert.Equal(product.Id, failedItem.ProductId);
			Assert.Equal(product.SKU, failedItem.SKU);
			Assert.Equal(expectedQuantityRequest, failedItem.QuantityRequest);
			Assert.Equal(expectedQuantityPrepared, failedItem.QuantityPrepared);
			Assert.Contains("Towar się nie zgadza", failedItem.Message);
		}
		[Fact]
		public async Task VerifyIssueToLoad_NotConfirmIssue_WhenNotExistIssue()
		{
			//Arrange
			//Act
			var issueId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			var result = await Mediator.Send(new VerifyIssueToLoadCommand(issueId2, "user123"));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Zamówienie nie zostało znalezione.", result.Error);
		}
	}
}
