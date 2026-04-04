using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.PickingPallets.Commands.ExecuteCorrectedPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class ExecutiveCorrectedPickingIntegrationTests : TestBase
	{
		//Metoda ExecutiveCorrectedPicking
		[Fact]
		public async Task ExecutiveManualPicking_WithIssueIdNewPallet_AssignsProductAndCommits()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);

			var product2 = Product.Create("Prod B", "777", 1, 100);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var pickingTask2= PickingTask.CreateForSeed(pickingGuid, 2, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
			//var pickingTask2 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product2.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
			//	PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
			//};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = newToPickPallet,
				InitialPalletQuantity = 20,
				LocationId = location1.Id,
				//Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask>()
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				LocationId = location1.Id,
				//Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			//pickingTask2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			await DbContext.SaveChangesAsync();
			// Act
			//var result = await _pickingPalletService.ExecuteManualPickingAsync(newToPickPallet.Id, issue.Id, "user1");
			var result = await Mediator.Send(new ExecuteCorrectedPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));
			// Assert		
			Assert.True(result.IsSuccess);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			// ✅ Paleta została zaktualizowana
			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.Issue)
				.FirstAsync(p => p.Id == newToPickPallet.Id);

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
			//Assert.Equal(PickingStatus.Allocated, pickingTaskAfter.PickingStatus);

			// Historia ruchu została zapisana 
			var history = await DbContext.HistoryPickings.ToListAsync();
			Assert.NotEmpty(history);
			Assert.Contains(history, h => h.PerformedBy == "user1" && h.PalletId == sourcePallet1.Id);

			// ✅ Walidacja, że kontekst nie trzyma niezatwierdzonych zmian
			Assert.False(DbContext.ChangeTracker.HasChanges());
		}
		[Fact]
		public async Task ExecutiveCorrectedPicking_WithIssueIdAddToOldPallet_AssignsProductAndCommits()
		{
			// Arrange
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);

			var product2 = Product.Create("Prod B", "777", 1, 100);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var oldPalletPallet = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPalletPallet.AddProductForTests(product2.Id, 10, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet, oldPalletPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid, 2, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
			//var pickingTask2 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product2.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
			//	PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
			//};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = newToPickPallet,
				InitialPalletQuantity = 20,
				LocationId = location1.Id,
				//Location = newToPickPallet.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask>()
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				LocationId = location1.Id,
				//Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			//pickingTask2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			await DbContext.SaveChangesAsync();
			// Act

			var result = await Mediator.Send(new ExecuteCorrectedPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));

			// Assert

			Assert.True(result.IsSuccess);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

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
			//.FirstOrDefaultAsync(a => a.Id == );

			Assert.NotNull(pickingTaskNew);
			Assert.Equal(PickingStatus.Picked, pickingTaskNew.PickingStatus);
			// ✅ Historia ruchu została zapisana 
			var history = await DbContext.HistoryPickings.ToListAsync();
			Assert.NotEmpty(history);

			// ✅ Walidacja, że kontekst nie trzyma niezatwierdzonych zmian
			Assert.False(DbContext.ChangeTracker.HasChanges());
		}
		[Fact]
		public async Task ExecutiveManualPicking_WithIssueIdNewPalletNewVirtualPallet_AssignsProductAndCommits()
		{
			// Arrange
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);

			var product2 = Product.Create("Prod B", "777", 1, 100);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);

			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid, 2, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
			//var pickingTask2 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated,
			//	ProductId = product2.Id,
			//	BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
			//	PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
			//};
			var virtualPallet2 = new VirtualPallet
			{
				Id =2,
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				LocationId = location1.Id,
				//Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			//pickingTask2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet2);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await Mediator.Send(new ExecuteCorrectedPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));
			// Assert		
			Assert.True(result.IsSuccess);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			// ✅ Paleta została zaktualizowana
			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Include(p => p.Issue)
				.FirstAsync(p => p.Id == newToPickPallet.Id);

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
			//Assert.Equal(PickingStatus.Allocated, pickingTaskAfter.PickingStatus);

			// ✅ Historia ruchu została zapisana (jeśli masz historię)
			var history = await DbContext.HistoryPickings.ToListAsync();
			Assert.NotEmpty(history);
			Assert.Contains(history, h => h.PerformedBy == "user1" && h.PalletId == sourcePallet1.Id);

			// ✅ Walidacja, że kontekst nie trzyma niezatwierdzonych zmian
			Assert.False(DbContext.ChangeTracker.HasChanges());
		}
		[Fact]
		public async Task ExecutiveCorrectedPicking_WithIssueIdDeleteIssue_ThrowException()
		{
			// Arrange
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 100);

			var product2 = Product.Create("Prod B", "777", 1, 100);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			await DbContext.SaveChangesAsync();
			var pickingGuid = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid, 2, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 null, null, null, 0);
			//var pickingTask2 = new PickingTask
			//{
			//	Issue = issue,
			//	RequestedQuantity = 10,
			//	PickingStatus = PickingStatus.Allocated
			//};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = newToPickPallet,
				InitialPalletQuantity = 20,
				LocationId = location1.Id,
				//Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask>()
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				LocationId = location1.Id,
				//Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			//pickingTask2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			DbContext.Issues.Remove(issue);
			await DbContext.SaveChangesAsync();

			// Act
			var result = await Mediator.Send(new ExecuteCorrectedPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));
			// Assert
			Assert.False(result.IsSuccess);
			Assert.Equal($"Zamówienie o numerze {issue.Id} nie zostało znalezione.", result.Error);
		}
	}
}
