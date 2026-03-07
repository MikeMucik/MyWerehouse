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
	public class IssueCompletedIntegrationServiceTests :TestBase 
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var pallet = new Pallet
			{
				Id = "P1",
				Location = location,
				Status = PalletStatus.Loaded,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				Id = "P2",
				Location = location,
				Status = PalletStatus.Loaded,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber =1,
				PickingTasks = new List<PickingTask>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.ConfirmedToLoad,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet , pallet1}
			};
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var pallet = new Pallet
			{
				Id = "P1",
				Location = location,
				Status = PalletStatus.Loaded,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				Id = "P2",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber =1,
				PickingTasks = new List<PickingTask>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.ConfirmedToLoad,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet, pallet1 }
			};
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
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
