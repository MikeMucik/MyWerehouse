using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Picking.Commands.DoPlannedPicking;
using MyWerehouse.Application.Picking.Commands.FinishPlannedPickingPrepareToHandPicking;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PickingPalletTests.Integration
{
	public class FinishPlannedPickingPrepareToHandPickingIntegrationTests : TestBase
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
			return Product.Create(name, sku, 1, 100);
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
		public async Task FinishPlannedPickingPrepareToHandPicking_ShouldCreateHandPickingForMissingQuantities_WhenNoPickingTaskDone()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location = CreateLocation(1, 1);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));

			var sourcePallet2 = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));

			var sourcePallet3 = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet3.AddProductForTests(product1.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddHours(12)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 20, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet2.Id, 10, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var virtualPallet3 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet3.Id, 20, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var pickingGuid1 = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid1, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			var pickingGuid2 = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid2, virtualPallet2.Id, issue.Id, 10, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			var pickingGuid3 = Guid.NewGuid();
			var pickingTask3 = PickingTask.CreateForSeed(pickingGuid3, virtualPallet3.Id, issue.Id, 15, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);

			DbContext.PickingTasks.AddRange(pickingTask1, pickingTask2, pickingTask3);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			DbContext.SaveChanges();
			//Act 
			var result = await Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand("User", null, null));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(2, result.Result.Count);
			//var resultForProduct1 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product1.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			//var resultForProduct2 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product2.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			var resultForProduct1 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product1.Id && x.IssueId == issue.Id && x.PickingStatus == PickingStatus.Available);
			var resultForProduct2 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product2.Id && x.IssueId == issue.Id && x.PickingStatus == PickingStatus.Available);
			Assert.NotNull(resultForProduct1);
			Assert.NotNull(resultForProduct2);
			Assert.Equal(25, resultForProduct1.RequestedQuantity);
			Assert.Equal(10, resultForProduct2.RequestedQuantity);
		}
		[Fact]
		public async Task FinishPlannedPickingPrepareToHandPicking_ShouldCreateHandPickingForMissingQuantities_WhenSomePickingWasAlreadyDone()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));

			var sourcePallet2 = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));

			var sourcePallet3 = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet3.AddProductForTests(product1.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)));

			var issueId = Guid.NewGuid();

			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddHours(12)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 20, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet2.Id, 10, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var virtualPallet3 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet3.Id, 20, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var pickingGuid1 = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid1, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			var pickingGuid2 = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid2, virtualPallet2.Id, issue.Id, 10, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			var pickingGuid3 = Guid.NewGuid();
			var pickingTask3 = PickingTask.CreateForSeed(pickingGuid3, virtualPallet3.Id, issue.Id, 15, PickingStatus.Allocated, product1.Id,
			 DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			DbContext.PickingTasks.AddRange(pickingTask1, pickingTask2, pickingTask3);
			DbContext.SaveChanges();
			//Act 1 
			var pickingTaskDTO = new PickingTaskDTO
			{
				Id = pickingTask2.Id,
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ProductId = pickingTask2.ProductId,
				RequestedQuantity = pickingTask2.RequestedQuantity,
				PickedQuantity = 10,
				PickingStatus = pickingTask2.PickingStatus,
				SourcePalletId = pickingTask2.VirtualPallet.PalletId,
				SourcePalletNumber = pickingTask2.VirtualPallet.Pallet.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask2.BestBefore,
			};
			var result1 = await Mediator.Send(new DoPlannedPickingCommand(pickingTaskDTO, "user1st"));
			Assert.True(result1.IsSuccess);
			//Act 
			var result = await Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand("user", null, null));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(2, result.Result.Count);

			var availableTasks = DbContext.PickingTasks
			.Where(x => x.IssueId == issue.Id && x.PickingStatus == PickingStatus.Available)
			.ToList();

			Assert.Equal(2, availableTasks.Count);
			Assert.Contains(availableTasks, x => x.ProductId == product1.Id && x.RequestedQuantity == 15);
			Assert.Contains(availableTasks, x => x.ProductId == product2.Id && x.RequestedQuantity == 10);
			
			var canceledTasks = DbContext.PickingTasks
			.Where(x => x.IssueId == issue.Id && x.PickingStatus == PickingStatus.Cancelled)
			.ToList();

			Assert.Equal(2, canceledTasks.Count);
		}

		[Fact]
		public async Task FinishPlannedPickingPrepareToHandPicking_ShouldCreateHandPickingForMissingQuantities_WhenPlannedPickingIsPartiallyCompleted()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product1 = CreateProduct("Prod A", "666");
			var product2 = CreateProduct("Prod B", "777");
			var location = CreateLocation(1, 1);
			var locationPicking = CreateLocation(100100, 5);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet1.AddProductForTests(product2.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(400)));

			var sourcePallet2 = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet2.AddProductForTests(product1.Id, 20, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(400)));

			var sourcePallet3 = Pallet.CreateForTests("Q1002", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet3.AddProductForTests(product1.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(400)));
			//nowe
			var sourcePallet4 = Pallet.CreateForTests("Q1003", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet4.AddProductForTests(product1.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(400)));

			var sourcePallet5 = Pallet.CreateForTests("Q1004", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			sourcePallet5.AddProductForTests(product2.Id, 15, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(400)));

			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow,
			DateOnly.FromDateTime(DateTime.UtcNow.AddHours(12)), "TestUser", IssueStatus.New, null);

			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3, sourcePallet4, sourcePallet5);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();

			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet1.Id, 20, sourcePallet1.LocationId, new DateTime(2025, 8, 12));

			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet2.Id, 10, sourcePallet2.LocationId, new DateTime(2025, 8, 12));

			var virtualPallet3 = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet3.Id, 15, sourcePallet3.LocationId, new DateTime(2025, 8, 12));

			var pickingGuid1 = Guid.NewGuid();
			var pickingTask1 = PickingTask.CreateForSeed(pickingGuid1, virtualPallet1.Id, issue.Id, 10, PickingStatus.Allocated, product2.Id,
			DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			var pickingGuid2 = Guid.NewGuid();
			var pickingTask2 = PickingTask.CreateForSeed(pickingGuid2, virtualPallet2.Id, issue.Id, 10, PickingStatus.Allocated, product1.Id,
			DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			var pickingGuid3 = Guid.NewGuid();
			var pickingTask3 = PickingTask.CreateForSeed(pickingGuid3, virtualPallet3.Id, issue.Id, 15, PickingStatus.Allocated, product1.Id,
			DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 0);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			DbContext.PickingTasks.AddRange(pickingTask1, pickingTask2, pickingTask3);
			await DbContext.SaveChangesAsync();
			//Act 1 
			var pickingTask1DTO = new PickingTaskDTO
			{
				Id = pickingTask1.Id,
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ProductId = pickingTask1.ProductId,
				SKU = pickingTask1.Product.SKU,
				RequestedQuantity = pickingTask1.RequestedQuantity,
				PickedQuantity = 5,
				PickingStatus = pickingTask1.PickingStatus,
				SourcePalletId = pickingTask1.VirtualPallet.PalletId,
				SourcePalletNumber = pickingTask1.VirtualPallet.Pallet.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask1.BestBefore,
			};
			var result1 = await Mediator.Send(new DoPlannedPickingCommand(pickingTask1DTO, "user1st"));
			Assert.True(result1.IsSuccess);
			var task1 = DbContext.PickingTasks.FirstOrDefault(t=>t.Id == pickingTask1.Id);
			Assert.NotNull(task1);
			Assert.Equal(PickingStatus.PickedPartially, task1.PickingStatus);
			Assert.Equal(5, task1.PickedQuantity);
			var pickingTask2DTO = new PickingTaskDTO
			{
				Id = pickingTask2.Id,
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ProductId = pickingTask2.ProductId,
				RequestedQuantity = pickingTask2.RequestedQuantity,
				PickedQuantity = 10,
				PickingStatus = pickingTask2.PickingStatus,
				SourcePalletId = pickingTask2.VirtualPallet.PalletId,
				SourcePalletNumber = pickingTask2.VirtualPallet.Pallet.PalletNumber,
				RampNumber = 100100,
				BestBefore = pickingTask2.BestBefore,
			};
			var result2 = await Mediator.Send(new DoPlannedPickingCommand(pickingTask2DTO, "user1st"));
			Assert.True(result2.IsSuccess);
			var task2 = DbContext.PickingTasks.FirstOrDefault(t => t.Id == pickingTask2.Id);
			Assert.NotNull(task2);
			Assert.Equal(PickingStatus.Picked, task2.PickingStatus);
			Assert.Equal(10, task2.PickedQuantity);
			//Act 2
			var result = await Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand("user", null, null));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(2, result.Result.Count);

			var availableTasks = DbContext.PickingTasks
			.Where(x => x.IssueId == issue.Id && x.PickingStatus == PickingStatus.Available)
			.ToList();

			Assert.Equal(2, availableTasks.Count);
			Assert.Contains(availableTasks, x => x.ProductId == product1.Id && x.RequestedQuantity == 15);
			Assert.Contains(availableTasks, x => x.ProductId == product2.Id && x.RequestedQuantity == 5);

			var canceledTasks = DbContext.PickingTasks
			.Where(x => x.IssueId == issue.Id && x.PickingStatus == PickingStatus.Cancelled)
			.ToList();

			Assert.Equal(2, canceledTasks.Count);
			Assert.Contains(canceledTasks, x => x.Id == pickingTask3.Id);
			Assert.DoesNotContain(canceledTasks, x => x.Id == pickingTask2.Id);

			Assert.Contains(availableTasks, x =>
				x.ProductId == product1.Id &&
				x.RequestedQuantity == 15);

			Assert.Contains(availableTasks, x =>
				x.ProductId == product2.Id &&
				x.RequestedQuantity == 5);
		}		
	}
}
