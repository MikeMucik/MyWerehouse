using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PickingPalletTests.Integration
{
	public class ExecutiveEmergencyPickingIntegrationTests : TestBase
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
				Name = name,
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, 1, 100);
		}
		private Location CreateLocation(int id, int position)
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
			sourcePallet.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var newSourcePallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newSourcePallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(	DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet, newSourcePallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var vPSource = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 10, location1.Id, new DateTime(2025, 8, 12));

			var vPNewSource = VirtualPallet.CreateForSeed(Guid.NewGuid(), newSourcePallet.Id, 20, location1.Id, new DateTime(2025, 8, 12));			
			
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, vPSource.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
		
			DbContext.VirtualPallets.AddRange(vPNewSource, vPSource);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newSourcePallet.Id, issue.Id, "user1", 100100));
			// Assert		
			Assert.True(result.IsSuccess);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Weź nową paletę dla zlecenia. Towar:", result.Result.Message);

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

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var oldPalletPallet = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPalletPallet.AddProductForTests(product2.Id, 10, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
						
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
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
			DbContext.PickingTasks.Add(pickingTask2);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			DbContext.SaveChanges();
			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newToPickPallet.Id, issue.Id, "user1", 100100));
			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			Assert.False(result.Result.NewPalletCreated);
			Assert.Contains("Dołącz towar do starej palety kompletacyjnej. Towar:", result.Result.Message);

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
			sourcePallet.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var newSourcePallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newSourcePallet.AddProductForTests(product2.Id, 8, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);

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
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			// Act
			var result = await Mediator.Send(new ExecuteEmergencyPickingCommand(newSourcePallet.Id, issue.Id, "user1", 100100));
			// Assert		
			Assert.True(result.IsSuccess);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			Assert.True(result.Result.NewPalletCreated);
			Assert.Contains("Weź nową paletę dla zlecenia. Towar:", result.Result.Message);

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
			Assert.Equal(PickingStatus.Correction, pickingTaskAfter.PickingStatus);

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
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var newToPickPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			newToPickPallet.AddProductForTests(product2.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime( DateTime.UtcNow.AddDays(7)), "TestUser", IssueStatus.New, null);
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
			Assert.Equal($"Zamówienie o numerze {issue.Id} nie zostało znalezione.", result.Error);
		}
	}
}
