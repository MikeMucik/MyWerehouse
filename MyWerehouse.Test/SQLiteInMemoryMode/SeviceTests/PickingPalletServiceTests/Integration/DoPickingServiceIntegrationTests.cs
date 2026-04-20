using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class DoPlannedPickingServiceIntegrationTests : TestBase
	{
		[Fact]
		public async Task DoPlannedPickingAsync_HappyPath_AddToBasePickingDone()
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product = Product.Create("Prod A", "666", 1, 100);			
			var location = new Location
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
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.SaveChanges();

			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 30, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, DateTime.UtcNow,
			DateTime.Now.AddDays(1), "TestUser", IssueStatus.New, null);
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), location.Id, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			DbContext.Pallets.AddRange(sourcePallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 40, sourcePallet.LocationId,new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 30, PickingStatus.Allocated, product.Id,
				null, null, null, 0);
			//virtualPallet.PickingTasks = new List<PickingTask> { pickingTask };
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();
			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask.Id,
				//Id = pickingTask.PickingTaskNumber,							
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ProductId = product.Id,
				RequestedQuantity = pickingTask.RequestedQuantity,
				PickedQuantity = 30,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet.Id,
				SourcePalletNumber = sourcePallet.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask.BestBefore,
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedPickingTask = await DbContext.PickingTasks.FindAsync(pickingTask.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert PickingTask
			Assert.NotNull(updatedPickingTask);
			Assert.Equal(newPallet.Id, updatedPickingTask.PickingPalletId);
			Assert.Equal(PickingStatus.Picked, updatedPickingTask.PickingStatus);
			// Assert Source Pallet (powinno zostać 10)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(10, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			// Assert New Pallet (powinno powstać 30 sztuk na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Single(newPallet.ProductsOnPallet);
			Assert.Equal(product.Id, newPallet.ProductsOnPallet.First().ProductId);
			Assert.Equal(30, newPallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		//Cała paleta jest pobierana bo to końcówka palety
		[Fact]
		public async Task DoPickingAsync_HappyPathTakeWholePallet_AddToBasePickingDone()
		{
			// Arrange
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product = Product.Create("Prod A", "666", 1, 100);			
			var location = new Location
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 40, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.Now.AddDays(1), "TestUser", IssueStatus.Pending, null);
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			DbContext.Pallets.AddRange(sourcePallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 40, sourcePallet.LocationId, new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 40, PickingStatus.Allocated, product.Id,
				null, null, null, 0);
			//virtualPallet.PickingTasks = new List<PickingTask> { pickingTask };
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask.Id,
				//Id = pickingTask.PickingTaskNumber,				
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ProductId = product.Id,
				RequestedQuantity = 40,
				PickedQuantity = 40,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet.Id,
				SourcePalletNumber = sourcePallet.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask.BestBefore,
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedPickingTask = await DbContext.PickingTasks.FindAsync(pickingTask.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert PickingTask
			Assert.NotNull(updatedPickingTask);
			Assert.Equal(newPallet.Id, updatedPickingTask.PickingPalletId);
			Assert.Equal(PickingStatus.Picked, updatedPickingTask.PickingStatus);
			// Assert Source Pallet (powinno zostać 0)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(0, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Archived, updatedSourcePallet.Status);
			// Assert New Pallet (powinno powstać 40 sztuk na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Single(newPallet.ProductsOnPallet);
			Assert.Equal(product.Id, newPallet.ProductsOnPallet.First().ProductId);
			Assert.Equal(40, newPallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheSameProductToExistPickingPallet_AddToBasePickingDone()
		{
			// Arrange
			var category = new Category
			{
				Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product = Product.Create("Prod A", "666", 1, 100);			
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, DateTime.UtcNow,
			DateTime.Now.AddDays(1), "TestUser", IssueStatus.New, issueItem);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), location1.Id, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var oldPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPallet.AddProductForTests(product.Id, 10, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			DbContext.Pallets.AddRange(sourcePallet1, oldPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product.Id,
				DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)), null, null, 0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { pickingTask1 };
			DbContext.PickingTasks.AddRange(pickingTask1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask1.Id,
				IssueNumber = pickingTask1.Issue.IssueNumber,
				IssueId = issue.Id,
				ProductId = product.Id,
				RequestedQuantity = pickingTask1.RequestedQuantity,
				PickedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id,
				SourcePalletNumber = sourcePallet1.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask1.BestBefore,
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedPickingTask = await DbContext.PickingTasks.FindAsync(pickingTask1.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet1.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert PickingTask
			Assert.NotNull(updatedPickingTask);
			Assert.Equal(newPallet.Id, updatedPickingTask.PickingPalletId);
			Assert.Equal(PickingStatus.Picked, updatedPickingTask.PickingStatus);

			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(90, updatedSourcePallet.ProductsOnPallet.First().Quantity);

			Assert.NotNull(newPallet);
			Assert.Single(newPallet.ProductsOnPallet);
			Assert.Equal(product.Id, newPallet.ProductsOnPallet.First().ProductId);
			Assert.Equal(20, newPallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheAnotherProductToExistPickingPallet_AddToBasePickingDone()
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product1.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product2.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.Now.AddDays(1), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var oldPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
						
			DbContext.Pallets.AddRange(sourcePallet1, oldPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
				null, null, null, 0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { pickingTask1 };
			DbContext.PickingTasks.AddRange(pickingTask1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask1.Id,
				//Id = pickingTask1.PickingTaskNumber,
				IssueId = issue.Id,
				ProductId = product2.Id,
				RequestedQuantity = pickingTask1.RequestedQuantity,
				PickedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id,
				SourcePalletNumber = sourcePallet1.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask1.BestBefore,
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedPickingTask = await DbContext.PickingTasks.FindAsync(pickingTask1.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet1.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert PickingTask
			Assert.NotNull(updatedPickingTask);
			Assert.Equal(newPallet.Id, updatedPickingTask.PickingPalletId);
			Assert.Equal(PickingStatus.Picked, updatedPickingTask.PickingStatus);
			// Assert Source Pallet (powinno zostać 90)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(90, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			// Assert New Pallet (powinno powstać 10 sztuk jednego produktu i 10 sztuk drugiego produktu na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Equal(2, newPallet.ProductsOnPallet.Count);
			Assert.Equal(20, newPallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity);
			Assert.Equal(10, newPallet.ProductsOnPallet.First(p => p.ProductId == product2.Id).Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheAnotherProductToExistPickingPalletWithHistory_AddToBasePickingDone()
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product1.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product2.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.Now.AddDays(1), "TestUser", IssueStatus.New, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var oldPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			DbContext.Pallets.AddRange(sourcePallet1, oldPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
				null, null, null, 0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { pickingTask1 };
			DbContext.PickingTasks.AddRange(pickingTask1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask1.Id,
				IssueId = issue.Id,
				ProductId = product2.Id,
				RequestedQuantity = pickingTask1.RequestedQuantity,
				PickedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id,
				SourcePalletNumber = sourcePallet1.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask1.BestBefore,
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedPickingTask = await DbContext.PickingTasks.FindAsync(pickingTask1.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet1.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert PickingTask
			Assert.NotNull(updatedPickingTask);
			Assert.Equal(newPallet.Id, updatedPickingTask.PickingPalletId);
			Assert.Equal(PickingStatus.Picked, updatedPickingTask.PickingStatus);
			// Assert Source Pallet (powinno zostać 90)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(90, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			// Assert New Pallet (powinno powstać 10 sztuk jednego produktu i 10 sztuk drugiego produktu na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Equal(2, newPallet.ProductsOnPallet.Count);
			Assert.Equal(20, newPallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity);
			Assert.Equal(10, newPallet.ProductsOnPallet.First(p => p.ProductId == product2.Id).Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);

			// Assert Pallet Movements (historia zmian)
			var movements = await DbContext.PalletMovements
				.Where(m => m.PalletId == sourcePallet1.Id || m.PalletId == newPallet.Id)
				.ToListAsync();

			// powinny być 2 wpisy: jeden dla źródłowej palety, jeden dla kompletacyjnej
			Assert.Equal(2, movements.Count);

			// źródłowa paleta (powinna mieć ruch typu ToPicking)
			var sourceMovement = movements.FirstOrDefault(m => m.PalletId == sourcePallet1.Id);
			Assert.NotNull(sourceMovement);
			Assert.Equal(ReasonMovement.Picking, sourceMovement.Reason);
			Assert.Equal(PalletStatus.ToPicking, sourceMovement.PalletStatus);

			// paleta kompletacyjna (również powinna mieć ruch typu Picking)
			var newPalletMovement = movements.FirstOrDefault(m => m.PalletId == newPallet.Id);
			Assert.NotNull(newPalletMovement);
			Assert.Equal(ReasonMovement.Picking, newPalletMovement.Reason);
			Assert.Equal(PalletStatus.Picking, newPalletMovement.PalletStatus);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheAnotherProductToExistPickingPalletAndMakeNewPickingTask_AddToBasePickingDone()
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product1.Id, 20, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1)),
				IssueItem.CreateForSeed(2, issueId, product2.Id, 10, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.Now.AddDays(1), "TestUser", IssueStatus.New, issueItem);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var sourcePallet2 = Pallet.CreateForTests("Q12000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
		

			var oldPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
						
			DbContext.Pallets.AddRange(sourcePallet1, oldPallet, sourcePallet2);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), 0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { pickingTask1 };
			DbContext.PickingTasks.AddRange(pickingTask1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask1.Id,
				IssueId = issue.Id,
				ProductId = product2.Id,
				RequestedQuantity = pickingTask1.RequestedQuantity,
				PickedQuantity = 5,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id,
				SourcePalletNumber = sourcePallet1.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask1.BestBefore,
				//BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
		}
		//SadPath
		//ale tu jest false bo nie wystarczająco produktu do pobrania do pobrania
		[Fact]
		public async Task DoPickingAsync_SadPathAddTheAnotherProductToExistPickingPalletAndMakeNewPickingTask_NotEnoughProduct_ThrowInfo()
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem = new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product1.Id, 30, new DateOnly(2026, 1, 1), new DateTime(2025, 1, 1))
			};
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, issueItem);

			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var sourcePallet2 = Pallet.CreateForTests("Q12000", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product2.Id, 1, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			//za mało towaru na source2
			var oldPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Picking, null, issueId);
			oldPallet.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			DbContext.Pallets.AddRange(sourcePallet1, oldPallet, sourcePallet2);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 10, location1.Id, new DateTime(2025, 8, 12));
			var pickingGuid = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)), null, null, 0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { pickingTask1 };
			DbContext.PickingTasks.AddRange(pickingTask1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask1.Id,
				IssueId = issue.Id,
				ProductId = product2.Id,
				RequestedQuantity = pickingTask1.RequestedQuantity,
				PickedQuantity = 5,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id,
				SourcePalletNumber = sourcePallet1.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask1.BestBefore,
			};
			var result = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1"));

			// Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Nie ma więcej asortymentu", result.Error);
		}
	}
}
