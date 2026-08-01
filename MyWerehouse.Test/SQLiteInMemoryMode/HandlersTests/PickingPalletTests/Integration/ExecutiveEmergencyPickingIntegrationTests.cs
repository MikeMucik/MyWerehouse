using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PickingPalletTests.Integration
{
	public class ExecutiveEmergencyPickingIntegrationTests : TestBase
	{
		private static Client CreateClient()
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
		private static Category CreateCategory(string name)
		{
			return new Category
			{
				Name = name,
				IsDeleted = false
			};
		}
		private static Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, TestDates.UtcNow, 1, 100, 30, 30, 30, 30, "TestDetails");
		}
		private static Location CreateLocation(int id, int position)
		{
			return new Location
			{
				Id = id,
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		[Fact]
		public async Task ExecutiveEmergencyPicking_ShouldPickedProductFromNewSource_WhenNotPickingPlanned()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);			
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var newSourcePallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newSourcePallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(	TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet, newSourcePallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var vPSource = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 10, location1.Id, new DateTime(2025, 8, 12));

			var vPNewSource = VirtualPallet.CreateForSeed(Guid.NewGuid(), newSourcePallet.Id, 20, location1.Id, new DateTime(2025, 8, 12));			
			
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, vPSource.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), 0);
		
			DbContext.VirtualPallets.AddRange(vPNewSource, vPSource);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newSourcePallet.Id, issue.Id, "user1", 100100));
			// Assert		
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal("Product was added to the issue.", result.Message);

			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Take a new pallet for the issue. Product:", result.Result.Message);

			// ✅ Paleta została zaktualizowana
			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.Issue)
				.FirstAsync(p => p.Id == newSourcePallet.Id);

			Assert.NotNull(updatedPallet);
			Assert.Equal(PalletStatus.ToPicking, updatedPallet.Status);

			// ✅ Produkt na palecie pozostał ten sam
			var productOnPallet = updatedPallet.ProductsOnPallet.Single();
			Assert.Equal(product2.Id, productOnPallet.ProductId);
			Assert.Equal(10, productOnPallet.Quantity);

			// ✅ Sprawdzenie, że VirtualPallet powiązany jest z paletą
			var virtualLinked = await DbContext.VirtualPallets
				.Include(v => v.Pallet)
				.Include(v => v.PickingTasks)
				.FirstOrDefaultAsync(v => v.Pallet.Id == newSourcePallet.Id);

			Assert.NotNull(virtualLinked);
			Assert.Equal(newSourcePallet.Id, virtualLinked.Pallet.Id);

			// ✅ Alokacje nie zostały utracone
			var pickingTaskAfter = await DbContext.PickingTasks
				.Include(a => a.Issue)
				.Include(a => a.VirtualPallet)
				.FirstAsync(a => a.Id == pickingTask.Id);

			Assert.Equal(issue.Id, pickingTaskAfter.Issue.Id);
			Assert.NotNull(pickingTaskAfter.VirtualPallet);
			Assert.Equal(PickingStatus.Cancelled, pickingTaskAfter.PickingStatus);

			// Historia ruchu została zapisana 
			var history = await DbContext.HistoryPickings.ToListAsync();
			Assert.NotEmpty(history);
			Assert.Contains(history, h => h.PerformedBy == "user1" && h.PalletId == sourcePallet.Id);

			// ✅ Walidacja, że kontekst nie trzyma niezatwierdzonych zmian
			Assert.False(DbContext.ChangeTracker.HasChanges());
		}
		[Fact]
		public async Task ExecutiveEmergencyPicking_AddedProductToExistPickingPallet_WhenOldPickingPalletExist()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var oldPalletPallet = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPalletPallet.AddProductForTests(product2.Id, 10, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));
						
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet, oldPalletPallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), newToPickPallet.Id, 20, location1.Id, new DateTime(2025, 8, 12));
			
			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 20, location1.Id, new DateTime(2025, 8, 12));
			
			var pickingGuid = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid, virtualPallet2.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), 0);
			DbContext.PickingTasks.Add(pickingTask2);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			DbContext.SaveChanges();
			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));
			// Assert
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal("Product was added to the issue.", result.Message);

			Assert.False(result.Result.NewPalletCreated);
			Assert.Contains("Add the product to the existing picking pallet. Product:", result.Result.Message);

			// ✅ Paleta została zaktualizowana
			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.Issue)
				.FirstAsync(p => p.Id == newToPickPallet.Id);

			Assert.NotNull(updatedPallet);
			Assert.Equal(PalletStatus.ToPicking, updatedPallet.Status);

			var oldpickedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == oldPalletPallet.Id);
			Assert.NotNull(oldpickedPallet);
			Assert.Equal(20, oldPalletPallet.ProductsOnPallet.First().Quantity);
			// ✅ Produkt na palecie pozostał ten sam
			var productOnPallet = updatedPallet.ProductsOnPallet.Single();
			Assert.Equal(product2.Id, productOnPallet.ProductId);
			Assert.Equal(10, productOnPallet.Quantity);


			// ✅ Sprawdzenie, że VirtualPallet powiązany jest z paletą
			var virtualLinked = await DbContext.VirtualPallets
				.Include(v => v.Pallet)
				.Include(v => v.PickingTasks)
				.FirstOrDefaultAsync(v => v.Pallet.Id == newToPickPallet.Id);

			Assert.NotNull(virtualLinked);
			Assert.Equal(newToPickPallet.Id, virtualLinked.Pallet.Id);

			// ✅ Alokacje nie zostały utracone
			var pickingTaskAfter = await DbContext.PickingTasks
				.Include(a => a.Issue)
				.Include(a => a.VirtualPallet)
				.FirstAsync(a => a.Id == pickingTask2.Id);

			Assert.Equal(issue.Id, pickingTaskAfter.Issue.Id);
			Assert.NotNull(pickingTaskAfter.VirtualPallet);
			Assert.Equal(PickingStatus.Cancelled, pickingTaskAfter.PickingStatus); //20 == 20 -> Cancelled

			var pickingTaskNew = await DbContext.PickingTasks
				.Include(a => a.Issue)
				.Include(a => a.VirtualPallet)
				.OrderBy(a => a.Id)
				.FirstOrDefaultAsync(a => a.Id != pickingTask2.Id);

			Assert.NotNull(pickingTaskNew);
			Assert.Equal(PickingStatus.Picked, pickingTaskNew.PickingStatus);
			// ✅ Historia ruchu została zapisana 
			var history = await DbContext.HistoryPickings.ToListAsync();
			Assert.NotEmpty(history);

			// ✅ Walidacja, że kontekst nie trzyma niezatwierdzonych zmian
			Assert.False(DbContext.ChangeTracker.HasChanges());
		}
		[Fact]
		public async Task ExecutiveEmergencyPicking_CreateNewVirtualPallet_WhenNoVirtualPallet()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);
			
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var newSourcePallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newSourcePallet.AddProductForTests(product2.Id, 8, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet, newSourcePallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), 0);
			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newSourcePallet.Id, issue.Id, "user1", 100100));
			// Assert		
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal("Product was added to the issue.", result.Message);

			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Take a new pallet for the issue. Product:", result.Result.Message);

			// ✅ Paleta została zaktualizowana
			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.Issue)
				.FirstAsync(p => p.Id == newSourcePallet.Id);

			Assert.NotNull(updatedPallet);
			Assert.Equal(PalletStatus.Archived, updatedPallet.Status);

			// ✅ Produkt na palecie pozostał ten sam
			var productOnPallet = updatedPallet.ProductsOnPallet.Single();
			Assert.Equal(product2.Id, productOnPallet.ProductId);
			Assert.Equal(0, productOnPallet.Quantity);

			// ✅ Sprawdzenie, że VirtualPallet powiązany jest z paletą
			var virtualLinked = await DbContext.VirtualPallets
				.Include(v => v.Pallet)
				.Include(v => v.PickingTasks)
				.FirstOrDefaultAsync(v => v.Pallet.Id == newSourcePallet.Id);

			Assert.NotNull(virtualLinked);
			Assert.Equal(newSourcePallet.Id, virtualLinked.Pallet.Id);

			// ✅ Alokacje nie zostały utracone
			var pickingTaskAfter = await DbContext.PickingTasks
				.Include(a => a.Issue)
				.Include(a => a.VirtualPallet)
				.FirstAsync(a => a.Id == pickingTask.Id);

			Assert.Equal(issue.Id, pickingTaskAfter.Issue.Id);
			Assert.NotNull(pickingTaskAfter.VirtualPallet);
			Assert.Equal(PickingStatus.CorrectionPicking, pickingTaskAfter.PickingStatus);

			// ✅ Historia ruchu została zapisana (jeśli masz historię)
			var history = await DbContext.HistoryPickings.ToListAsync();
			Assert.NotEmpty(history);
			Assert.Contains(history, h => h.PerformedBy == "user1" && h.PalletId == sourcePallet.Id);

			// ✅ Walidacja, że kontekst nie trzyma niezatwierdzonych zmian
			Assert.False(DbContext.ChangeTracker.HasChanges());
		}
		[Fact]
		public async Task ExecutiveEmergencyPicking_ThrowException_WhenIssueIsDelete()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location1 = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);
			
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
			DateOnly.FromDateTime( TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();

			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), newToPickPallet.Id, 20, location1.Id, new DateTime(2025, 8, 12));
			
			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			
			var pickingGuid = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid, virtualPallet2.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 null, null, null, 0);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			DbContext.PickingTasks.Add(pickingTask2);
			DbContext.Issues.Remove(issue);//!!!!!
			DbContext.SaveChanges();

			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));
			// Assert
			Assert.False(result.IsSuccess);
			Assert.Equal($"Issue {issue.Id} was not found.", result.Error);
		}

		[Fact]
		public async Task ExecuteEmergencyPicking_ShouldFail_WhenIssueHasPickingShortage()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Prod A", "666");
			var location = CreateLocation(1, 1);

			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();

			var emergencyPallet = Pallet.CreateForTests("Q2000", new DateTime(2025, 8, 8),
				location.Id, PalletStatus.Available, null, null);
			emergencyPallet.AddProductForTests(product.Id, 10, new DateTime(2025, 8, 8),
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.PickingShortage, null);

			DbContext.Pallets.Add(emergencyPallet);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();
			//Act&Assert
			var ex = await Assert.ThrowsAsync<NotAllowedOperationDomainException>(() => Mediator.Send(new ExecuteEmergencyPickingCommand(
				emergencyPallet.Id, issue.Id, "user1", location.Id)));
			Assert.Contains($"Operation forbidden for {issue.IssueNumber}({issueId}), wrong status.", ex.Message);
			//// Act
			//var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(
			//	emergencyPallet.Id, issue.Id, "user1", location.Id));

			//// Assert
			//Assert.False(result.IsSuccess);
			//Assert.Equal(ErrorType.Conflict, result.ErrorType);
			//Assert.Equal("The issue status does not allow emergency picking.", result.Error);
			//Assert.Equal(PalletStatus.Available, emergencyPallet.Status);
			//Assert.Equal(10, emergencyPallet.ProductsOnPallet.Single().Quantity);
		}

		[Fact]
		public async Task ExecuteEmergencyPicking_ShouldCompleteCorrectionTask_WhenEmergencyIsExecutedSecondTime()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Prod A", "666");
			var location = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();

			var bestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365));
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8),
				location.Id, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 100, new DateTime(2025, 8, 8), bestBefore);

			var firstEmergencyPallet = Pallet.CreateForTests("Q2000", new DateTime(2025, 8, 8),
				location.Id, PalletStatus.Available, null, null);
			firstEmergencyPallet.AddProductForTests(product.Id, 4, new DateTime(2025, 8, 8), bestBefore);

			var secondEmergencyPallet = Pallet.CreateForTests("Q2001", new DateTime(2025, 8, 8),
				location.Id, PalletStatus.Available, null, null);
			secondEmergencyPallet.AddProductForTests(product.Id, 6, new DateTime(2025, 8, 8), bestBefore);

			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet, firstEmergencyPallet, secondEmergencyPallet);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();

			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 10,
				location.Id, new DateTime(2025, 8, 12));
			var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 10,
				PickingStatus.Allocated, product.Id, bestBefore, null,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(5)), 0);

			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();

			// Act - first Emergency Picking leaves 6 in CorrectionPicking
			var firstResult = await Mediator.Send(new ExecuteEmergencyPickingCommand(
				firstEmergencyPallet.Id, issue.Id, "user1", locationPicking.Id));

			Assert.True(firstResult.IsSuccess);
			Assert.Equal(PickingStatus.CorrectionPicking, pickingTask.PickingStatus);
			Assert.Equal(6, pickingTask.RequestedQuantity);

			// Act - second Emergency Picking completes the remaining quantity
			var secondResult = await Mediator.Send(new ExecuteEmergencyPickingCommand(
				secondEmergencyPallet.Id, issue.Id, "user1", locationPicking.Id));

			// Assert
			Assert.True(secondResult.IsSuccess);

			var pickingTaskAfter = await DbContext.PickingTasks
				.FirstAsync(a => a.Id == pickingTask.Id);
			Assert.Equal(PickingStatus.Cancelled, pickingTaskAfter.PickingStatus);
			Assert.Equal(0, pickingTaskAfter.RequestedQuantity);

			var pickingPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.SingleAsync(p => p.IssueId == issue.Id && p.Status == PalletStatus.Picking);
			Assert.Equal(10, pickingPallet.ProductsOnPallet.Single(p => p.ProductId == product.Id).Quantity);

			var emergencyTasks = await DbContext.PickingTasks
				.Where(t => t.IssueId == issue.Id && t.Id != pickingTask.Id && t.PickingStatus == PickingStatus.Picked)
				.ToListAsync();
			Assert.Equal(2, emergencyTasks.Count);
			Assert.Equal(10, emergencyTasks.Sum(t => t.PickedQuantity));
			Assert.Equal(PalletStatus.Archived, firstEmergencyPallet.Status);
			Assert.Equal(PalletStatus.Archived, secondEmergencyPallet.Status);
		}

		[Fact]
		public async Task ExecuteEmergencyPicking_ShouldFailValidation_WhenRampDoesNotExist()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Prod A", "666");
			var location = CreateLocation(1, 1);

			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();

			var bestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365));
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8),
				location.Id, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 100, new DateTime(2025, 8, 8), bestBefore);

			var emergencyPallet = Pallet.CreateForTests("Q2000", new DateTime(2025, 8, 8),
				location.Id, PalletStatus.Available, null, null);
			emergencyPallet.AddProductForTests(product.Id, 5, new DateTime(2025, 8, 8), bestBefore);

			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet, emergencyPallet);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();

			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 10,
				location.Id, new DateTime(2025, 8, 12));
			var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 10,
				PickingStatus.Allocated, product.Id, bestBefore, null,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(5)), 0);

			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();

			// Act - ramp does not exist
			var exception = await Assert.ThrowsAsync<ValidationException>(() =>
				Mediator.Send(new ExecuteEmergencyPickingCommand(
					emergencyPallet.Id, issue.Id, "user1", 999999)));

			// Assert
			Assert.Contains(exception.Errors, failure =>
				failure.PropertyName == nameof(ExecuteEmergencyPickingCommand.RampNumber) &&
				failure.ErrorMessage == "The selected location does not exist.");

			await using var freshContext = CreateNewContext();
			var emergencyPalletAfter = await freshContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.SingleAsync(p => p.Id == emergencyPallet.Id);
			var pickingTaskAfter = await freshContext.PickingTasks
				.SingleAsync(t => t.Id == pickingTask.Id);
			var tasksCount = await freshContext.PickingTasks.CountAsync(t => t.IssueId == issue.Id);

			Assert.Equal(PalletStatus.Available, emergencyPalletAfter.Status);
			Assert.Equal(5, emergencyPalletAfter.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PickingStatus.Allocated, pickingTaskAfter.PickingStatus);
			Assert.Equal(10, pickingTaskAfter.RequestedQuantity);
			Assert.Equal(1, tasksCount);
		}

		[Fact]
		public async Task ExecuteEmergencyPicking_ShouldThrow_WhenSourcePalletHasDifferentBestBefore()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Prod A", "666");
			var location = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);
			var expectedBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365));
			var sourceBestBefore = expectedBestBefore.AddDays(-30);

			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			await DbContext.SaveChangesAsync();

			var issue = Issue.CreateForSeed(
				Guid.NewGuid(),
				1,
				client.Id,
				TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)),
				"TestUser",
				IssueStatus.New,
				null);

			var allocatedSourcePallet = Pallet.CreateForTests(
				"Q1000",
				TestDates.UtcNow,
				location.Id,
				PalletStatus.ToPicking,
				null,
				null);
			allocatedSourcePallet.AddProductForTests(
				product.Id,
				10,
				TestDates.UtcNow,
				expectedBestBefore);

			var emergencySourcePallet = Pallet.CreateForTests(
				"Q1001",
				TestDates.UtcNow,
				location.Id,
				PalletStatus.ToPicking,
				null,
				null);
			emergencySourcePallet.AddProductForTests(
				product.Id,
				5,
				TestDates.UtcNow,
				sourceBestBefore);

			DbContext.Pallets.AddRange(allocatedSourcePallet, emergencySourcePallet);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			var allocatedVirtualPallet = VirtualPallet.CreateForSeed(
				Guid.NewGuid(),
				allocatedSourcePallet.Id,
				10,
				location.Id,
				TestDates.UtcNow);
			var emergencyVirtualPallet = VirtualPallet.CreateForSeed(
				Guid.NewGuid(),
				emergencySourcePallet.Id,
				5,
				location.Id,
				TestDates.UtcNow);
			var allocatedTask = PickingTask.CreateForSeed(
				Guid.NewGuid(),
				allocatedVirtualPallet.Id,
				issue.Id,
				10,
				PickingStatus.Allocated,
				product.Id,
				expectedBestBefore,
				null,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(5)),
				0);

			DbContext.VirtualPallets.AddRange(allocatedVirtualPallet, emergencyVirtualPallet);
			DbContext.PickingTasks.Add(allocatedTask);
			await DbContext.SaveChangesAsync();

			// Act
			await Assert.ThrowsAsync<NotCorrectDateBestBeforeDomainException>(() =>
				Mediator.Send(new ExecuteEmergencyPickingCommand(
					emergencySourcePallet.Id,
					issue.Id,
					"UserCor",
					locationPicking.Id)));

			// Assert
			await using var freshContext = CreateNewContext();
			var emergencyPalletAfter = await freshContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.SingleAsync(p => p.Id == emergencySourcePallet.Id);
			var allocatedTaskAfter = await freshContext.PickingTasks
				.SingleAsync(t => t.Id == allocatedTask.Id);
			var issueAfter = await freshContext.Issues
				.SingleAsync(i => i.Id == issue.Id);
			var emergencyVirtualPalletAfter = await freshContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.SingleAsync(v => v.Id == emergencyVirtualPallet.Id);

			Assert.Equal(PalletStatus.ToPicking, emergencyPalletAfter.Status);
			Assert.Equal(5, emergencyPalletAfter.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PickingStatus.Allocated, allocatedTaskAfter.PickingStatus);
			Assert.Equal(10, allocatedTaskAfter.RequestedQuantity);
			Assert.Equal(IssueStatus.New, issueAfter.IssueStatus);
			Assert.Empty(emergencyVirtualPalletAfter.PickingTasks);
			Assert.False(await freshContext.Pallets.AnyAsync(p =>
				p.IssueId == issue.Id &&
				p.Status == PalletStatus.Picking));
			Assert.Equal(1, await freshContext.PickingTasks.CountAsync(t => t.IssueId == issue.Id));
		}
	}
}
