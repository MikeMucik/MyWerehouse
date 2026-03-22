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
using MyWerehouse.Domain.Picking.Models;
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var pallet = new Pallet
			{
				PalletNumber = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				PalletNumber = "P2",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 2,
				PickingTasks = new List<PickingTask>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.Pending,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet }
			};
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
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
			Assert.Equal(PalletStatus.InTransit, p2.Status);
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
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var pallet = new Pallet
			{
				PalletNumber = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				PalletNumber = "P2",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 2,
				PickingTasks = new List<PickingTask>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.Pending,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet }
			};
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
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
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var pallet = new Pallet
			{
				PalletNumber = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				PalletNumber = "P2",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			//var issue = new Issue
			//{
			//	Id = Guid.NewGuid(),
			//	IssueNumber = 2,
			//	PickingTasks = new List<PickingTask>(),
			//	Client = client,
			//	IssueItems = new List<IssueItem> { new IssueItem
			//{
			//	Product = product,
			//	Quantity = 20,
			//	BestBefore = new DateOnly(2026, 1, 1)
			//}},
			//	IssueStatus = IssueStatus.Pending,
			//	PerformedBy = "user1",
			//	IssueDateTimeCreate = DateTime.Now.AddDays(-7),
			//	IssueDateTimeSend = DateTime.Now.AddDays(1),
			//	Pallets = new List<Pallet> { pallet }
			//};
			//pallet.Issue = issue;
			//pallet.IssueId = issue.Id;
			DbContext.Pallets.AddRange(pallet, pallet1);
			//DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
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
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var productA = new Product { Name = "ProdA", SKU = "A", Category = category };
			var pallet = new Pallet
			{
				PalletNumber = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				PalletNumber = "P2",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = productA, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				PickingTasks = new List<PickingTask>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.Pending,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet }
			};
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
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
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var pallet = new Pallet
			{
				PalletNumber = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				PalletNumber = "P2",
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
				IssueStatus = IssueStatus.Pending,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet }
			};
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			//Act
			//var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var result = await Mediator.Send(new ChangePalletInIssueCommand(issue.Id, pallet.Id, pallet1.Id, "tester"));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Contains("błędny status", result.Error);
		}
	}
}

