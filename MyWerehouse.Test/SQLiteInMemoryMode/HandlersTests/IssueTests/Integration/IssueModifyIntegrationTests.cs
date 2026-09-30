using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.Commands.ModifyIssue;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pickings.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.IssueTests.Integration
{
	public class IssueModifyIntegrationTests : TestBase
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
		private static Product CreateProduct(string name, int categoryId)
		{
			return Product.Create(name, "SKU1", TestDates.UtcNow, categoryId, 10, 30, 30, 30, 30, "TestDetails");
		}
		private static Location CreateLocation(int position)
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
		public async Task ModifyIssue_ShouldCreatePickingTasks_WhenNoneExistAtCreation()
		{
			// Arrange – setup initial data
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore =DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365))  }
				}
			};
			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Act 2: update the order to 15 units (1 full pallet + 5 units to pick)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(1));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366))  }
		}
			};
			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));

			// Assert the issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, updatedIssue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be one allocation (5 units) linked to the VirtualPallet for "P2"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(5, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc.VirtualPallet.PalletId);

			// Additional check: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.Single(v => v.PalletId == pallet2.Id);

			Assert.Equal(5, vp.PickingTasks.First().RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Single(result.Result.Results);
			Assert.True(result.Result.Results.Single().Success);
			Assert.Equal(product.Id, result.Result.Results.Single().ProductId);

			var p2After = DbContext.Pallets.AsNoTracking().Single(p => p.PalletNumber == "P2");
			// Safety check: confirm that the status actually changed
			Assert.Equal(PalletStatus.ToPicking, p2After.Status);
		}
		[Fact]
		public async Task ModifyIssue_ShouldCreatePickingTasks_WhenExistAtCreation()
		{
			// Arrange – setup initial data
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);

			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet and a picking task (12 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore =DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365))  }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert 1
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.NotNull(issue);
			Assert.Single(issue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be one allocation (2 units) linked to the VirtualPallet for "P2"
			Assert.Single(pickingTasksForIssue1);
			var alloc1 = pickingTasksForIssue1.Single();
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc1.VirtualPallet.PalletId);

			var history = DbContext.HistoryPickings
				.OrderBy(h => h.DateTime)
				.ToList();
			// There should be 1 entry: Create
			Assert.NotNull(history);
			Assert.Single(history);
			// Act 2: update the order to 15 units (1 full pallet + 5 units to pick)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User2",
				IssueItems = new List<IssueItemDTO>
					{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore =DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365))  }
					}
			};
			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, updatedIssue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();
			// There should be one allocation (5 units) linked to the VirtualPallet for "P2"
			Assert.Single(pickingTasksForIssue);
			var pickingTask = pickingTasksForIssue.Single();
			Assert.Equal(5, pickingTask.RequestedQuantity);
			Assert.NotNull(pickingTask.VirtualPallet);
			Assert.Equal(pallet2.Id, pickingTask.VirtualPallet.PalletId);
			//Check that history was saved
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == pallet2.Id);
			Assert.Equal(5, vp.PickingTasks.First().RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);
			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Single(result.Result.Results);
			Assert.True(result.Result.Results.First().Success);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);

			//Assert
			var historyPallets = DbContext.HistoryPallet
				.Where(h => h.PalletNumber == "P2")
				.ToList();
			// Assert – historia alokacji po aktualizacji
			var history1 = DbContext.HistoryPickings
				.OrderBy(h => h.DateTime)
				.ToList();
			// There should be 3 entries: Create + Cancel + Create
			Assert.Equal(3, history1.Count);

			// The last entry should be Correction
			var firstHistory = history1.Skip(1).First();
			Assert.Equal(PickingStatus.Cancelled, firstHistory.StatusAfter);
			var lastHistory = history1.Last();
			Assert.Equal(PickingStatus.Allocated, lastHistory.StatusAfter);
			Assert.Equal("User2", lastHistory.PerformedBy);
			Assert.Equal(pickingTask.Id, lastHistory.PickingTaskId);
		}
		[Fact]
		public async Task ModifyIssue_ShouldCreatePickingTasks_WhenOtherPickingTasksExistOnSourcePallet()
		{
			// Arrange – setup initial data
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.ToPicking, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			var issueId = Guid.NewGuid();

			var issueOld = Issue.CreateForSeed(issueId, 1, 1, TestDates.Now.AddDays(-10),
			DateOnly.FromDateTime(TestDates.Now.AddDays(2)), "userS", IssueStatus.InProgress, null);

			var sourcePallet = VirtualPallet.Create(pallet2.Id, pallet2.ProductsOnPallet.First().Quantity, 2, TestDates.UtcNow);
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, sourcePallet.Id, issueId, 4, PickingStatus.Allocated, product.Id,
					null, null, null, 0);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Issues.AddRange(issueOld);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(sourcePallet);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};
			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert 1
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).FirstOrDefault(i => i.IssueNumber == 2);
			Assert.NotNull(issue);
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			// Act 2: update the order to 15 units (1 full pallet + 5 units to pick)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				ClientId = issue.ClientId,
				PerformedBy = "User2",
				IssueItems = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
		}
			};
			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));
			// Assert the issue
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.IssueNumber == issue.IssueNumber);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, updatedIssue.Pallets.First().Status);
			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();
			// There should be one allocation (5 units) linked to the VirtualPallet for "P2"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(5, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc.VirtualPallet.PalletId);
			// Additional check: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == pallet2.Id);

			Assert.Equal(5, vp.PickingTasks.First(x => x.IssueId == issue.Id).RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);
			Assert.Equal(1, vp.RemainingQuantity);
			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Single(result.Result.Results);
			Assert.True(result.Result.Results.First().Success);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);
		}
		[Fact]
		public async Task ModifyIssue_ShouldMakeNewIssue_WhenIssueConfirmedToLoadAndOldIssueExist()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.ToPicking, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var existingIssueId = Guid.NewGuid();
			var existingIssueItem = IssueItem.CreateForSeed(1, existingIssueId, product.Id, 4, null, TestDates.UtcNow.AddDays(1));
			var existingIssue = Issue.CreateForSeed(existingIssueId, 1, 1, TestDates.Now.AddDays(-10),
			DateOnly.FromDateTime(TestDates.Now.AddDays(2)), "userS", IssueStatus.InProgress, [existingIssueItem]);
			var virtualPallet = VirtualPallet.Create(pallet2.Id, pallet2.ProductsOnPallet.First().Quantity, 2, TestDates.UtcNow);
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, existingIssueId, 4, PickingStatus.Allocated, product.Id,
				null, null, null, 0);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Issues.AddRange(existingIssue);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();
			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			Assert.NotNull(created.Result);
			var issueCreated = DbContext.Issues
				.AsNoTracking()
				.Include(i => i.Pallets)
				.FirstOrDefault(i => i.IssueNumber == 2);//IssueNumber = 1 refers to the original issue
			Assert.NotNull(issueCreated);
			Assert.Single(issueCreated.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issueCreated.Pallets.First().Status);
			//Act1.1
			var resultVerify = await Mediator.Send(new VerifyIssueToLoadCommand(issueCreated.Id, "userV"));
			//Assert 1.1
			Assert.True(resultVerify.IsSuccess);
			var issueAfter = DbContext.Issues.Find(issueCreated.Id);
			var pallet1After = DbContext.Pallets.Find(pallet1.Id);
			Assert.Equal(PalletStatus.ToIssue, pallet1After!.Status);
			Assert.NotNull(issueAfter);
			Assert.Equal(IssueStatus.ConfirmedToLoad, issueAfter.IssueStatus);
			var savedIssue = await DbContext.Issues
				.Include(i => i.Pallets).ThenInclude(p => p.ProductsOnPallet)
				.Include(i => i.IssueItems)
				.FirstOrDefaultAsync(i => i.Id == issueCreated.Id);
			Assert.NotNull(savedIssue);
			Assert.Single(savedIssue.Pallets);
			Assert.Single(savedIssue.IssueItems);
			Assert.All(savedIssue.Pallets, p => Assert.True(p.ProductsOnPallet.Any()));
			var comparison = Assert.Single(resultVerify.Result!);
			Assert.Equal(10, comparison.QuantityRequest);
			Assert.Equal(10, comparison.QuantityPrepared);

			// Act 2: update the order to 15 units (1 full pallet + 5 units to pick)

			var id = issueCreated.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User2",
				IssueItems = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
		}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			var newIssueItems = DbContext.IssueItems.Where(i => i.IssueId == issueCreated.Id).ToList();
			foreach (var it in newIssueItems)
			{
				Console.WriteLine($"Item: ProductId={it.ProductId}, Quantity={it.Quantity}, BestBefore={it.BestBefore}");
			}
			// Assert the issue: issueNumber is 3 because this is a new supplementary issue
			var newIssue = DbContext.Issues.First(i => i.IssueNumber == 3);
			var newNumberGuid = DbContext.Issues.Single(i => i.IssueNumber == 3).Id;
			var newIssueItems1 = DbContext.IssueItems.Where(i => i.IssueId == newNumberGuid).ToList();
			Assert.NotEqual(issueCreated.Id, result.Result.IssueId);
			Assert.Equal(result.Result.IssueId, newNumberGuid);
			Assert.NotEqual(issueCreated.IssueNumber, result.Result.IssueNumber);
			Assert.Equal(result.Result.IssueNumber, newIssue.IssueNumber);
			Assert.NotNull(newIssue);  // The issue exists
			Assert.Single(newIssueItems1);  // Exactly one!
			Assert.Equal(product.Id, newIssueItems1.Single().ProductId);
			Assert.Equal(5, newIssueItems1.Single().Quantity);  // Difference
			Assert.Equal(DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)), newIssueItems1.Single().BestBefore);

			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.IssueNumber == 3); //Third issue in the test

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Empty(updatedIssue.Pallets);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == updatedIssue.Id)
				.ToList();

			// There should be one allocation (5 units) linked to the VirtualPallet for "P2"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(5, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc.VirtualPallet.PalletId);

			// Additional check: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == pallet2.Id);

			Assert.Equal(5, vp.PickingTasks.First(x => x.IssueId == updatedIssue.Id).RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);
			Assert.Equal(1, vp.RemainingQuantity);

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Single(result.Result.Results);
			Assert.True(result.Result.Results.First().Success);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);
		}

		[Fact]
		public async Task ModifyIssue_ShouldAddOneProductToIssue_WhenInsufficientForOneProduct()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product1 = CreateProduct("Prod1", 1);
			var product2 = CreateProduct("Prod2", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet3.AddProduct(product2.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product1.Id, Quantity = 12, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) },
					new IssueItemDTO { ProductId = product2.Id, Quantity = 7, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be two allocations (2 units) linked to the VirtualPallets for "P2" and "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Single(a => a.ProductId == product1.Id);
			var alloc2 = pickingTasksForIssue1.Single(a => a.ProductId == product2.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(7, alloc2.RequestedQuantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc1.VirtualPallet.PalletId);
			Assert.Equal(pallet3.Id, alloc2.VirtualPallet.PalletId);

			// Act 2: update the order to 22 units (insufficient stock)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product1.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) } ,
					new IssueItemDTO { ProductId = product2.Id, Quantity = 8, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);



			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result.Results);
			Assert.Equal(2, result.Result.Results.Count);
			Assert.False(result.Result.Results.First().Success);
			Assert.True(result.Result.Results.Last().Success);
			Assert.Contains($"Insufficient quantity of product {product1.Id}", result.Result.Results.First().Message);
			Assert.Equal(product1.Id, result.Result.Results.First().ProductId);
			Assert.Equal(product2.Id, result.Result.Results.Last().ProductId);
			Assert.Equal("User2", updatedIssue.PerformedBy);
		}

		[Fact]
		public async Task ModifyIssue_ShouldUpdateIsuue_WhenSufficientStaffForBothProducts()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var location2 = CreateLocation(3);
			var location3 = CreateLocation(4);
			var product1 = CreateProduct("Prod1", 1);
			var product2 = CreateProduct("Prod2", 1);

			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet4 = Pallet.CreateForTests("P4", TestDates.UtcNow, 4, PalletStatus.Available, null, null);
			pallet4.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product2.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location, location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet4, pallet3);
			await DbContext.SaveChangesAsync();

			// Act 1 
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product1.Id, Quantity = 12, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) },
					new IssueItemDTO { ProductId = product2.Id, Quantity = 7, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be two allocations (2 units) linked to the VirtualPallets for "P2" and "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Single(a => a.ProductId == product1.Id);
			var alloc2 = pickingTasksForIssue1.Single(a => a.ProductId == product2.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(7, alloc2.RequestedQuantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc1.VirtualPallet.PalletId);
			Assert.Equal(pallet3.Id, alloc2.VirtualPallet.PalletId);

			// Act 2: update the order to 21 units (insufficient stock)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,

				IssueItems = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product1.Id, Quantity = 21, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) } ,
					new IssueItemDTO { ProductId = product2.Id, Quantity = 8, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result.Results);
			Assert.Equal(2, result.Result.Results.Count);
			Assert.True(result.Result.Results.First().Success);
			Assert.True(result.Result.Results.Last().Success);
			Assert.Contains($"Product {product1.SKU} was added to the issue.", result.Result.Results.First().Message);
			Assert.Contains($"Product {product2.SKU} was added to the issue.", result.Result.Results.Last().Message);
			Assert.Equal(product1.Id, result.Result.Results.First().ProductId);
			Assert.Equal(product2.Id, result.Result.Results.Last().ProductId);

			var updatedIssue1 = DbContext.Issues
				.Include(i => i.Pallets)
				.Include(i => i.PickingTasks) // Load the allocations as well!
				.First(i => i.Id == issue.Id);

			// CHECK PRODUCT 1 (21 units)
			// Expected: 2 full pallets + an allocation of 1 unit
			var palletsProd1 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product1.Id))
				.ToList();

			Assert.Equal(2, palletsProd1.Count); // There should be 2 pallets (e.g. P1 and P4)

			var allocProd1 = updatedIssue1.PickingTasks.Single(a => a.ProductId == product1.Id);

			Assert.Equal(1, allocProd1.RequestedQuantity);

			// CHECK PRODUCT 2 (8 units)
			// Expected: 0 full pallets + an allocation of 8 units
			var palletsProd2 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product2.Id))
				.ToList();
			Assert.Empty(palletsProd2); // 8 units do not make a full pallet

			var allocProd2 = updatedIssue1.PickingTasks
				.FirstOrDefault(a => a.ProductId == product2.Id);
			Assert.NotNull(allocProd2);
			Assert.Equal(8, allocProd2.RequestedQuantity);
		}

		[Fact]
		public async Task ModifyIssue_ShouldReduceProductQuantituty_WhenQuantityIsLowerThanOriginal()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var location2 = CreateLocation(3);
			var location3 = CreateLocation(4);
			var product = CreateProduct("Prod1", 1);
			var product1 = CreateProduct("Prod2", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(13)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(13)));
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(13)));
			var pallet4 = Pallet.CreateForTests("P4", TestDates.UtcNow, 4, PalletStatus.Available, null, null);
			pallet4.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(13)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location, location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 7, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Equal(2, issue.Pallets.Count); // P1 and P2 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be two allocations (2 units) linked to the VirtualPallets for "P2" and "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Single(a => a.ProductId == product.Id);
			var alloc2 = pickingTasksForIssue1.Single(a => a.ProductId == product1.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(7, alloc2.RequestedQuantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal(pallet4.Id, alloc1.VirtualPallet.PalletId);
			Assert.Equal(pallet3.Id, alloc2.VirtualPallet.PalletId);

			// Act 2: update the order to 11 units
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 11, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)) } ,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 8, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(300)) }
				}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result.Results);
			Assert.Equal(2, result.Result.Results.Count);
			Assert.True(result.Result.Results.First().Success);
			Assert.True(result.Result.Results.Last().Success);
			Assert.Contains($"Product {product.SKU} was added to the issue.", result.Result.Results.First().Message);
			Assert.Contains($"Product {product.SKU} was added to the issue.", result.Result.Results.Last().Message);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);
			Assert.Equal(product1.Id, result.Result.Results.Last().ProductId);

			var updatedIssue1 = DbContext.Issues
				.Include(i => i.Pallets)
				.Include(i => i.PickingTasks) // Load the allocations as well!
				.First(i => i.Id == issue.Id);

			// CHECK PRODUCT 1 (11 units)
			// Expected: 1 full pallet + an allocation of 1 unit
			var palletsProd1 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product.Id))
				.ToList();

			Assert.Single(palletsProd1); // There should be 1 pallet (e.g. P1)

			var allocProd1 = updatedIssue1.PickingTasks.FirstOrDefault(a => a.ProductId == product.Id);
			Assert.NotNull(allocProd1);
			Assert.Equal(1, allocProd1.RequestedQuantity);

			// CHECK PRODUCT 2 (8 units)
			// Expected: 0 full pallets + an allocation of 8 units
			var palletsProd2 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product1.Id))
				.ToList();
			Assert.Empty(palletsProd2); // 8 units do not make a full pallet

			var allocProd3 = updatedIssue1.PickingTasks
				.FirstOrDefault(a => a.ProductId == product1.Id);
			Assert.NotNull(allocProd3);
			Assert.Equal(8, allocProd3.RequestedQuantity);
		}
		// Sad path
		[Fact]
		public async Task ModifyIssueAsync_ReturnInfo_WhenInsufficientStaff()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			Assert.NotNull(created.Result);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be one allocation (2 units) linked to the VirtualPallet for "P2"
			Assert.Single(pickingTasksForIssue1);
			var alloc1 = pickingTasksForIssue1.Single();
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc1.VirtualPallet.PalletId);

			// Act 2: update the order to 22 units (insufficient stock)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
		}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User1", updatedIssue.PerformedBy); //The operation failed, so the previous user remains

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Single(result.Result.Results);
			Assert.False(result.Result.Results.First().Success);
			Assert.Contains($"Insufficient quantity of product {product.Id}", result.Result.Results.First().Message);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);
		}
		[Fact]
		public async Task ModifyIssueAsync_WrongStatusPallet()
		{
			// Arrange – setup initial data
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 1, PalletStatus.OnHold, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			Assert.NotNull(created.Result);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Act 2: update the order to 22 units (insufficient stock)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
		}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User1", updatedIssue.PerformedBy); //The operation failed, so the user from the previous change remains

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Single(result.Result.Results);
			Assert.False(result.Result.Results.First().Success);
			Assert.Contains($"Insufficient quantity of product {product.Id}. The product was not added to the issue.", result.Result.Results.First().Message);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);
		}
		[Fact]
		public async Task ModifyIssueAsync_NotIssue_ThrowsException()
		{
			// Arrange – setup initial data
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 1, PalletStatus.OnHold, null, null);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert 1
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.NotNull(issue);
			Assert.Single(issue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Act 2 – update: inny numer id
			var id = Guid.NewGuid();
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User2",
				IssueItems = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};
			// Assert & Act
			var result1 = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));
			Assert.NotNull(result1);
			Assert.False(result1.IsSuccess);
			Assert.Contains($"Issue was not found.", result1.Error);
		}
		// Not completed after update
		[Fact]
		public async Task ModifyIssueAsync_IssueNotCompleted_WhenSecondProductEnoughFirstNot()
		{
			// Arrange – setup initial data
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var location2 = CreateLocation(3);
			var location3 = CreateLocation(4);
			var product1 = CreateProduct("Prod1", 1);
			var product2 = CreateProduct("Prod2", 1);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location, location1, location2);
			DbContext.SaveChanges();
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location.Id, PalletStatus.Available, null, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location1.Id, PalletStatus.Available, null, null);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, location2.Id, PalletStatus.Available, null, null);
			pallet3.AddProduct(product2.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));


			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();

			// Act 1 
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product1.Id, Quantity = 12, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) },
					new IssueItemDTO { ProductId = product2.Id, Quantity = 8, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};
			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));
			//Assert 1
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			Assert.NotNull(created.Result);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Act 2: update product1 to 2 pallets + 2 units; product2 remains unchanged
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product1.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) },
					new IssueItemDTO { ProductId = product2.Id, Quantity = 8, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365))}
				}
			};
			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Results);
			Assert.Equal(2, result.Result.Results.Count);
			Assert.False(result.Result.Results.First(x => x.ProductId == product1.Id).Success);
			Assert.True(result.Result.Results.First(x => x.ProductId == product2.Id).Success);
			// Assert the issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);
			Assert.Equal(IssueStatus.RequiresCorrection, updatedIssue.IssueStatus);
			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be one allocation (8 units) linked to the VirtualPallet for "P3"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(8, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal(pallet3.Id, alloc.VirtualPallet.PalletId);

			// Additional check: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == pallet3.Id);

			Assert.Equal(8, vp.PickingTasks.First().RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);

			// ACT UpdateIssueAsync
			var p1After = DbContext.Pallets.AsNoTracking().Single(p => p.PalletNumber == "P1");
			var p2After = DbContext.Pallets.AsNoTracking().Single(p => p.PalletNumber == "P2");
			var p3After = DbContext.Pallets.AsNoTracking().Single(p => p.PalletNumber == "P3");
			// Safety check: confirm that the status actually changed
			Assert.Equal(PalletStatus.LockedForIssue, p1After.Status);
			Assert.Equal(PalletStatus.Available, p2After.Status);
			Assert.Equal(PalletStatus.ToPicking, p3After.Status);
		}
		[Fact]
		public async Task ModifyIssueAsync_IssueNotCompleted_WhenSecondProductReducedFirstNotEnough()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("name");
			var location = CreateLocation(1);
			var location1 = CreateLocation(2);
			var location2 = CreateLocation(3);
			var location3 = CreateLocation(4);
			var product = CreateProduct("Prod1", 1);
			var product1 = CreateProduct("Prod2", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.Available, null, null);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location, location1, location2);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();

			// Act 1: create an issue with 1 pallet (10 units)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 2, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7))));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // P1 should be assigned
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);

			// Assert allocations assigned to this issue (check the PickingTasks table)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp!.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// There should be two allocations (2 units) linked to the VirtualPallets for "P2" and "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Find(a => a.ProductId == product.Id);
			Assert.NotNull(alloc1);
			var alloc2 = pickingTasksForIssue1.Find(a => a.ProductId == product1.Id);
			Assert.NotNull(alloc2);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(2, alloc2.RequestedQuantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal(pallet2.Id, alloc1.VirtualPallet.PalletId);
			Assert.Equal(pallet3.Id, alloc2.VirtualPallet.PalletId);

			// Act 2: update the order to 22 units (insufficient stock)
			var id = issue.Id;
			var dateToSend = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7));
			var updateDto = new ModifyIssueDTO
			{
				PerformedBy = "User2",
				ClientId = client.Id,
				IssueItems = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) } ,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 3, BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(365)) }
				}
			};

			var result = await Mediator.Send(new ModifyIssueCommand(id, updateDto, dateToSend));

			// Assert the issue
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// The UpdateIssueAsync result should include a result for the product
			Assert.NotNull(result.Result.Results);
			Assert.Equal(2, result.Result.Results.Count);
			Assert.False(result.Result.Results.First().Success);
			Assert.True(result.Result.Results.Last().Success);
			Assert.Contains($"Insufficient quantity of product {product.Id}", result.Result.Results.First().Message);
			Assert.Equal(product.Id, result.Result.Results.First().ProductId);
			Assert.Equal(product1.Id, result.Result.Results.Last().ProductId);
		}

		[Fact]
		public async Task ModifyIssue_ShouldRollbackFirstSave_WhenReallocationThrowsException()
		{
			// Arrange – create an issue with one full pallet and one picking task
			var client = CreateClient();
			var category = CreateCategory("name");
			var location1 = CreateLocation(1);
			var location2 = CreateLocation(2);
			var product = CreateProduct("Prod1", 1);

			var pallet1 = Pallet.CreateForTests(
				"P1",
				TestDates.UtcNow,
				locationId: 1,
				PalletStatus.Available,
				receiptId: null,
				issueId: null);
			pallet1.AddProduct(
				product.Id,
				quantity: 10,
				TestDates.UtcNow,
				TestDates.Today.AddDays(366));

			var pallet2 = Pallet.CreateForTests(
				"P2",
				TestDates.UtcNow,
				locationId: 2,
				PalletStatus.Available,
				receiptId: null,
				issueId: null);
			pallet2.AddProduct(
				product.Id,
				quantity: 10,
				TestDates.UtcNow,
				TestDates.Today.AddDays(366));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2);
			DbContext.Products.Add(product);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			var createDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items =
				[
					new IssueItemDTO
					{
						ProductId = product.Id,
						Quantity = 12,
						BestBefore = TestDates.Today.AddDays(365)
					}
				]
			};
			var created = await Mediator.Send(new CreateIssueCommand(
				createDto,
				TestDates.Today.AddDays(7)));

			Assert.True(created.IsSuccess);
			Assert.NotNull(created.Result);
			var issueId = created.Result.IssueId;

			DbContext.ChangeTracker.Clear();
			var pickingTaskBefore = await DbContext.PickingTasks
				.AsNoTracking()
				.SingleAsync(task => task.IssueId == issueId);
			Assert.Equal(PickingStatus.Allocated, pickingTaskBefore.PickingStatus);

			var expectedException = new InvalidOperationException(
				"Controlled failure after the first SaveChangesAsync.");
			var throwingAssignService = new Mock<IAssignProductToIssueService>();
			throwingAssignService
				.Setup(service => service.AssignGoodsToIssue(
					It.IsAny<Issue>(),
					It.IsAny<IssueItemDTO>(),
					It.IsAny<IssueAllocationPolicy>(),
					It.IsAny<List<Pallet>?>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(expectedException);

			var handler = new ModifyIssueHandler(
				_provider.GetRequiredService<IIssueRepo>(),
				Mediator,
				_provider.GetRequiredService<IUnitOfWork>(),
				throwingAssignService.Object,
				_provider.GetRequiredService<IVirtualPalletRepo>(),
				_provider.GetRequiredService<IDateTimeProvider>());

			var modifyDto = new ModifyIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User2",
				IssueItems =
				[
					new IssueItemDTO
					{
						ProductId = product.Id,
						Quantity = 15,
						BestBefore = TestDates.Today.AddDays(365)
					}
				]
			};

			// Act – the exception occurs after PrepareForReallocation was saved
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				handler.Handle(
					new ModifyIssueCommand(
						issueId,
						modifyDto,
						TestDates.Today.AddDays(7)),
					CancellationToken.None));

			// Assert – read again after clearing stale tracked state
			Assert.Same(expectedException, exception);
			throwingAssignService.Verify(
				service => service.AssignGoodsToIssue(
					It.IsAny<Issue>(),
					It.IsAny<IssueItemDTO>(),
					It.IsAny<IssueAllocationPolicy>(),
					It.IsAny<List<Pallet>?>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()),
				Times.Once);

			DbContext.ChangeTracker.Clear();
			var issueAfterRollback = await DbContext.Issues
				.AsNoTracking()
				.Include(issue => issue.Pallets)
				.SingleAsync(issue => issue.Id == issueId);
			var pallet1AfterRollback = await DbContext.Pallets
				.AsNoTracking()
				.SingleAsync(pallet => pallet.Id == pallet1.Id);
			var pallet2AfterRollback = await DbContext.Pallets
				.AsNoTracking()
				.SingleAsync(pallet => pallet.Id == pallet2.Id);
			var pickingTaskAfterRollback = await DbContext.PickingTasks
				.AsNoTracking()
				.SingleAsync(task => task.Id == pickingTaskBefore.Id);

			Assert.Equal(IssueStatus.Pending, issueAfterRollback.IssueStatus);
			Assert.Equal("User1", issueAfterRollback.PerformedBy);
			Assert.Single(issueAfterRollback.Pallets);
			Assert.Equal(pallet1.Id, issueAfterRollback.Pallets.Single().Id);
			Assert.Equal(issueId, pallet1AfterRollback.IssueId);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1AfterRollback.Status);
			Assert.Null(pallet2AfterRollback.IssueId);
			Assert.Equal(PalletStatus.ToPicking, pallet2AfterRollback.Status);
			Assert.Equal(PickingStatus.Allocated, pickingTaskAfterRollback.PickingStatus);
		}
	}
}
