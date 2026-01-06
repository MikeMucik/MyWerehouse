using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueChangePalltIntegrationServiceTests : IssueIntegrationCommandService
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
				Id = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				Id = "P2",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Allocations = new List<Allocation>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				//ProductId = product.Id,
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
			var result = await _issueService.ChangePalletInIssueAsync(issue.Id, "P1", "P2", "tester");

			// Assert
			Assert.True(result.Success);
			Assert.Equal("Podmieniono palety.", result.Message);

			var updatedIssue = await DbContext.Issues.Include(i => i.Pallets).FirstAsync(i => i.Id == issue.Id);
			var p1 = await DbContext.Pallets.FirstAsync(p => p.Id == "P1");
			var p2 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");

			Assert.DoesNotContain(updatedIssue.Pallets, p => p.Id == "P1");
			Assert.Contains(updatedIssue.Pallets, p => p.Id == "P2");

			Assert.Equal(PalletStatus.Available, p1.Status);
			Assert.Equal(PalletStatus.ToIssue, p2.Status);
			Assert.Equal(IssueStatus.ChangingPallet, updatedIssue.IssueStatus);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenSamePalletIds()
		{
			var result = await _issueService.ChangePalletInIssueAsync(1, "P1", "P1", "tester");
			Assert.False(result.Success);
			Assert.Contains("tą samą", result.Message);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenIssueNotFound()
		{
			var result = await _issueService.ChangePalletInIssueAsync(999, "P1", "P2", "tester");
			Assert.False(result.Success);
			Assert.Contains("Zamówienie o numerze 999 nie zostało znal", result.Message);
		}

		[Fact]
		public async Task ChangePalletDuringLoadingAsync_ShouldFail_WhenDifferentProducts()
		{
			// Arrange
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
				Id = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
			};
			var pallet1 = new Pallet
			{
				Id = "P2",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>{
							new ProductOnPallet { Product = productA, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Allocations = new List<Allocation>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				//ProductId = product.Id,
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
			
			
			
			
			var productB = new Product { Name = "ProdB", SKU = "B", Category = category };
			pallet.Issue = issue;
			pallet.IssueId = issue.Id;
			DbContext.Pallets.AddRange(pallet, pallet1);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();


			// Act
			var result = await _issueService.ChangePalletInIssueAsync(issue.Id, "P1", "P2", "tester");

			// Assert
			Assert.False(result.Success);
			Assert.Contains("różnymi produktami", result.Message);
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
				Id = "P1",
				Location = location,
				Status = PalletStatus.ToIssue,
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
				Allocations = new List<Allocation>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				//ProductId = product.Id,
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

			var result = await _issueService.ChangePalletInIssueAsync(1, "P1", "P2", "tester");

			Assert.False(result.Success);
			Assert.Contains("błędny status", result.Message);
		}
	}
}
	
