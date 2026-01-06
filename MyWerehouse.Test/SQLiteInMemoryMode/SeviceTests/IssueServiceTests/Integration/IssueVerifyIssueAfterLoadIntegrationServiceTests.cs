using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueVerifyIssueAfterLoadIntegrationServiceTests : IssueIntegrationCommandService
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var product1 = new Product { Name = "Prod2", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
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
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Allocations = new List<Allocation>(),
				Client = client,
			//	IssueItems = new List<IssueItem> { new IssueItem
			//{
			//	Product = product,
			//	Quantity = 20,
			//	BestBefore = new DateOnly(2026, 1, 1)
			//}},
				IssueStatus = IssueStatus.IsShipped,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet, pallet1 }
			};
			pallet.Issue = issue;
			pallet1.Issue = issue;
			pallet.IssueId = issue.Id;
			pallet1.IssueId = issue.Id;
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
			var result = await _issueService.VerifyIssueAfterLoadingAsync(issue.Id, "UserTest");
			//Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
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
				.Include(h=>h.Details)
				.FirstAsync(i=>i.IssueId == issue.Id);
			Assert.NotNull(issueHistory);
			var palletHistory = await DbContext.PalletMovements
				.Include(p=>p.PalletMovementDetails)
				.FirstAsync(ph=>ph.PalletId == pallet.Id);
			Assert.NotNull(palletHistory);
			var palletHistory1 = await DbContext.PalletMovements
				.Include(p => p.PalletMovementDetails)
				.FirstAsync(ph => ph.PalletId == pallet1.Id);
			Assert.NotNull(palletHistory1);
		}
		[Fact]
		public async Task VerifyIssueAfterLoadingAsync_IssueNotFound_ReturnFail()
		{
			//Act
			var result = await _issueService.VerifyIssueAfterLoadingAsync(999, "userX");

			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Equal("Zamówienie o numerze 999 nie zostało znalezione.", result.Message);
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var product1 = new Product { Name = "Prod2", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
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
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Allocations = new List<Allocation>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.InProgress,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet, pallet1 }
			};
			pallet.Issue = issue;
			pallet1.Issue = issue;
			pallet.IssueId = issue.Id;
			pallet1.IssueId = issue.Id;
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
			var result = await _issueService.VerifyIssueAfterLoadingAsync(issue.Id, "userX");

			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Equal("Nie wszystkie palety mają status Loaded.", result.Message);
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var product1 = new Product { Name = "Prod2", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
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
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
			};
			var issue = new Issue
			{
				Allocations = new List<Allocation>(),
				Client = client,
				IssueItems = new List<IssueItem> { new IssueItem
			{
				Product = product,
				Quantity = 20,
				BestBefore = new DateOnly(2026, 1, 1)
			}},
				IssueStatus = IssueStatus.InProgress,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet, pallet1 }
			};
			pallet.Issue = issue;
			pallet1.Issue = issue;
			pallet.IssueId = issue.Id;
			pallet1.IssueId = issue.Id;
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
			var result = await _issueService.VerifyIssueAfterLoadingAsync(issue.Id, "userX");

			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Equal("Nie zakończono załadunku.", result.Message);
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
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
			var product1 = new Product { Name = "Prod2", SKU = "SKU2", Category = category, CartonsPerPallet = 20 };
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
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
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
				IssueStatus = IssueStatus.IsShipped,
				PerformedBy = "user1",
				IssueDateTimeCreate = DateTime.Now.AddDays(-7),
				IssueDateTimeSend = DateTime.Now.AddDays(1),
				Pallets = new List<Pallet> { pallet, pallet1 }
			};
			DbContext.Issues.Add(issue);
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
			await DbContext.SaveChangesAsync();

			// Act
			var result = await _issueService.VerifyIssueAfterLoadingAsync(issue.Id, "user1");
			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
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
		//[Fact]
		//public async Task VerifyIssueAfterLoadingAsync_InventoryServiceThrowsException_ReturnFailAndRollback()
		//{
		//	//Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var client = new Client { Name = "TestCompany", Email = "123@op.pl", Description = "Description", FullName = "FullNameCompany", Addresses = new List<Address> { address } };
		//	var category = new Category { Name = "Cat" };
		//	var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
		//	var product = new Product { Name = "Prod1", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
		//	var product1 = new Product { Name = "Prod2", SKU = "SKU1", Category = category, CartonsPerPallet = 10 };
		//	var pallet = new Pallet
		//	{
		//		Id = "P1",
		//		Location = location,
		//		Status = PalletStatus.ToIssue,
		//		ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026, 1, 1) } }
		//	};
		//	var pallet1 = new Pallet
		//	{
		//		Id = "P2",
		//		Location = location,
		//		Status = PalletStatus.ToIssue,
		//		ProductsOnPallet = new List<ProductOnPallet>{
		//					new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }}
		//	};
		//	var issue = new Issue
		//	{
		//		Allocations = new List<Allocation>(),
		//		Client = client,
		//		IssueItems = new List<IssueItem> { new IssueItem
		//	{
		//		Product = product,
		//		Quantity = 20,
		//		BestBefore = new DateOnly(2026, 1, 1)
		//	}},
		//		IssueStatus = IssueStatus.IsShipped,
		//		PerformedBy = "user1",
		//		IssueDateTimeCreate = DateTime.Now.AddDays(-7),
		//		IssueDateTimeSend = DateTime.Now.AddDays(1),
		//		Pallets = new List<Pallet> { pallet, pallet1 }
		//	};
		//	pallet.Issue = issue;
		//	pallet1.Issue = issue;
		//	pallet.IssueId = issue.Id;
		//	pallet1.IssueId = issue.Id;
		//	//Inventory
		//	//dla dwóch produktów
		//	var inventory = new Inventory
		//	{
		//		Product = product,
		//		LastUpdated = DateTime.Now.AddDays(-7),
		//		Quantity = 100
		//	};
		//	var inventory1 = new Inventory
		//	{
		//		Product = product1,
		//		LastUpdated = DateTime.Now.AddDays(-7),
		//		Quantity = 100
		//	};
		//	DbContext.AddRange(inventory, inventory1);
		//	DbContext.Pallets.AddRange(pallet, pallet1);
		//	DbContext.Issues.Add(issue);
		//	await DbContext.SaveChangesAsync();
		//	//Podmieniamy usługę inventory by rzucała wyjątek
		//	var mockInventoryService = new Mock<IInventoryService>();
		//	mockInventoryService
		//		.Setup(x => x.ChangeProductQuantityAsync(It.IsAny<int>(), It.IsAny<int>()))
		//		.ThrowsAsync(new Exception("Inventory update failed"));

		//	var issueService = new IssueService(
		//		Mediator,
		//		_issueRepo,
		//		_mapper,
		//		DbContext,
		//		//_historyService,
		//		mockInventoryService.Object,
		//		_palletRepo,
		//		_productRepo,
		//		_allocationRepo,
		//		_pickingPalletRepo,
		//		_palletService,
		//		_issueItemRepo,
		//		_createIssueValidator
		//		,_eventCollector
		//		);
		//	//Act
		//	var result = await issueService.VerifyIssueAfterLoadingAsync(issue.Id, "userX");

		//	//Assert
		//	Assert.NotNull(result);
		//	Assert.False(result.Success);
		//	Assert.Equal("Wystąpił nieoczenikawy błąd przy weryfikacji", result.Message);

		//	await using var newContext = CreateNewContext();
		//	//Rollback — status zlecenia i palet nie powinien się zmienić
		//	var issueFromDb = await newContext.Issues.Include(i => i.Pallets).FirstAsync(i => i.Id == issue.Id);
		//	Assert.Equal(IssueStatus.IsShipped, issueFromDb.IssueStatus);
		//	Assert.All(issueFromDb.Pallets, p => Assert.Equal(PalletStatus.ToIssue, p.Status));
		//}
	}
}
