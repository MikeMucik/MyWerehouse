using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Picking.Commands.DoPlannedPicking;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.ReversePickingTests.Integration
{
	public class ExecutiveReversePickingTests : TestBase
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
			return Product.Create(name, sku, 1, 10);
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
		private Receipt CreateReceipt()
		{
			return	Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMake",	DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 100100);
		}
		
		[Fact]
		public async Task ExecutiveReversePicking_ShouldProductBackToSourcePallet_WhenOneProduct()
		{
			// Arrange	
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location1 = CreateLocation(1, 1);
			var location2 = CreateLocation(2, 2);
			var locationBase = CreateLocation(100100, 5);			
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, locationBase);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18,BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))  }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).Single();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Single(pickingTasksToDo);
			//Act 2 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x=>x.ProductId == product.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = "P2",
				ProductId = product.Id,
				RampNumber = locationBase.Id,

			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(8, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);

			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);

			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert 3 - result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);
			//Assert 3 reversePickingTasks
			var reverseTasks = await DbContext.ReversePickings
				.ToListAsync();
			Assert.Single(reverseTasks);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);

			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(2, palletP2.LocationId);
			// Assert – picingTask
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.ReturnToSource,
				pickingPallet.Id, "UserReverse", null, null));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.IsSuccess);
			Assert.NotNull(resultReversePicking.Result);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar do palety źródłowej", resultReversePicking.Result.Message);
			Assert.Equal(pallet2.Id, resultReversePicking.Result.PalletId);
			var palletAfterRerversePicking = await DbContext.Pallets
				.Include(pp=>pp.ProductsOnPallet)
				.SingleAsync(p => p.PalletNumber == "P2");
			Assert.NotNull(palletAfterRerversePicking);
			Assert.Equal(10, palletAfterRerversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterRerversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);

			var reverseAfter = await DbContext.ReversePickings.ToListAsync();

			Assert.Contains(reverseAfter, x =>
				x.Id == task.Id &&
				x.Status == ReversePickingStatus.Completed);

			var history = DbContext.HistoryPickings;
			Assert.NotNull(history);
			Assert.Equal(2, history.Count());
			var history1 = DbContext.HistoryReversePickings;
			Assert.NotNull(history1);
			Assert.Equal(2, history1.Count());
		}
		[Fact]
		public async Task ExecuteReversePicking_ShouldRestoreProductToNewPallet_WhenOneProduct()
		{
			// Arrange 	
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location1 = CreateLocation(1, 1);
			var location2 = CreateLocation(2, 2);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, locationBase);

			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Single(pickingTasksToDo);
			//Act 2 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x => x.ProductId == product.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = "P2",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert
			Assert.NotNull(resultPicking);
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(8, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);
			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);
			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert 3 result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);
			//Assert 3 reversePickingTasks
			var reverseTasks = await DbContext.ReversePickings
				.ToListAsync();
			Assert.Single(reverseTasks);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);
			var reverseTaskForProduct = await DbContext.ReversePickings
			.Where(rp => rp.SourcePalletId == pallet2.Id)
			.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			// Assert – Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(2, palletP2.LocationId);			
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToNewPallet,
				pickingPallet.Id, "UserReverse", null, 100100));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.IsSuccess);
			Assert.NotNull(resultReversePicking.Result);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar do nowej palety.", resultReversePicking.Result.Message);
			var palletAfteReversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.PalletNumber == "Q0002");

			Assert.NotNull(palletAfteReversePicking);
			Assert.Equal(8, palletAfteReversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.InStock, palletAfteReversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);

			var history = DbContext.HistoryPickings;
			Assert.NotNull(history);
			Assert.Equal(2, history.Count());
			var history1 = DbContext.HistoryReversePickings;
			Assert.NotNull(history1);
			Assert.Equal(2, history1.Count());

			var reverseAfter = await DbContext.ReversePickings.ToListAsync();

			Assert.Contains(reverseAfter, x =>
				x.Id == task.Id &&
				x.Status == ReversePickingStatus.Completed);
		}
		[Fact]
		public async Task ExecuteReversePicking_ShouldRestoreProductToExistPallet_WhenOneProduct()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location1 = CreateLocation(1, 1);
			var location2 = CreateLocation(2, 2);
			var location3 = CreateLocation(3, 3);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, locationBase, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert
			Assert.NotNull(created);
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Single(pickingTasksToDo);
			//Act 2 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x => x.ProductId == product.Id);
			//foreach (var pickingFromBase in pickingsFromBase) {
				var toPicking = new PickingTaskDTO
				{
					Id = pickingTaskForProduct.Id,
					PickingStatus = pickingTaskForProduct.PickingStatus,
					BestBefore = pickingTaskForProduct.BestBefore,
					RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
					PickedQuantity = 8,
					SourcePalletId = pallet2.Id,
					SourcePalletNumber = "P2",
					ProductId = product.Id,
					RampNumber = 100100,

				}; 
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2
			Assert.NotNull(resultPicking);
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(8, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);
			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);
			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert 3
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);
			//Assert 3 reversePickingTasks
			var reverseTasks = await DbContext.ReversePickings
				.ToListAsync();
			Assert.Single(reverseTasks);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);
			var reverseTaskForProduct = await DbContext.ReversePickings
			.Where(rp => rp.SourcePalletId == pallet2.Id)
			.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			// Assert – Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(2, palletP2.LocationId);

			//Arange for Act 4
			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Pallets.Add(pallet3);
			await DbContext.SaveChangesAsync();
			var existingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "P3");
			var list = new List<Pallet> { existingPallet };
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToExistingPallet,
				pickingPallet.Id, "UserReverse", list, null));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.IsSuccess);
			Assert.NotNull(resultReversePicking.Result);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar.", resultReversePicking.Result.Message);
			var palletAfterReversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.PalletNumber == "P3");
			var listPalletIdsToAdd = resultReversePicking.Result.PalletWithAddedProduct
				.Select(x => x.PalletId)
				.ToList();			
			Assert.NotNull(palletAfterReversePicking);
			Assert.Contains(palletAfterReversePicking.Id, listPalletIdsToAdd);
			Assert.Equal(9, palletAfterReversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterReversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);

			var history = DbContext.HistoryPickings;
			Assert.NotNull(history);
			Assert.Equal(2, history.Count());
			var history1 = DbContext.HistoryReversePickings;
			Assert.NotNull(history1);
			Assert.Equal(2, history1.Count());

			var reverseAfter = await DbContext.ReversePickings.ToListAsync();

			Assert.Contains(reverseAfter, x =>
				x.Id == task.Id &&
				x.Status == ReversePickingStatus.Completed);
		}
		[Fact]
		public async Task ExecuteReversePicking_ShouldRestoreProductToExistingPalletAndArchivePickingPallet_WhenNoOtherPickedProductRemains()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var product1 = CreateProduct("Prod2", "SKU2");
			var location1 = CreateLocation(1, 1);
			var location2 = CreateLocation(2, 2);
			var location3 = CreateLocation(3, 3);
			var location4 = CreateLocation(4, 4);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet4 = Pallet.CreateForTests("P4", DateTime.UtcNow, 4, PalletStatus.Available, receipt.Id, null);
			pallet4.AddProduct(product1.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location1, locationBase, location2, location3, location4);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet4);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18,BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))  },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 4, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);
			Assert.Equal(PalletStatus.ToPicking, pallet4.Status);
			var pickingTaskToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTaskToDo);
			Assert.Equal(2, pickingTaskToDo.Count);
			//Act 2 - wykonanie pickingu
			var pickingTaskForProduct = pickingTaskToDo.Single(p => p.ProductId == product.Id); // await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = pallet2.PalletNumber,
				ProductId = product.Id,
				RampNumber = 100100,

			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(8, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);

			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);
			
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert 3
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);
			//Assert 3  reversePickingTask for product
			var reverseTasks = await DbContext.ReversePickings
				.ToListAsync();
			Assert.Single(reverseTasks);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);

			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);

			var palletP4 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P4");
			Assert.Equal(PalletStatus.Available, palletP4.Status);
			Assert.Equal(10, pallet4.ProductsOnPallet.Single().Quantity);
			
			//ArrangeFor Act4
			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Pallets.Add(pallet3);
			await DbContext.SaveChangesAsync();
			var existingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "P3");
			var list = new List<Pallet> { existingPallet };			
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToExistingPallet,
				pickingPallet.Id, "UserReverse", list, null));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.IsSuccess);
			Assert.NotNull(resultReversePicking.Result);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar.", resultReversePicking.Result.Message);
			var palletAfterReversePicking = await DbContext.Pallets
				.Include(pp=>pp.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.PalletNumber == "P3");
			Assert.NotNull(palletAfterReversePicking);
			var listPalletIdsToAdd = resultReversePicking.Result.PalletWithAddedProduct
				.Select(x => x.PalletId)
				.ToList();
			Assert.Contains(palletAfterReversePicking.Id, listPalletIdsToAdd);
			Assert.NotNull(palletAfterReversePicking);
			const int existingQuantityOnTargetPallet = 1;
			const int pickedQuantityToReverse = 8;
			const int expectedQuantityAfterReverse = existingQuantityOnTargetPallet + pickedQuantityToReverse;
			Assert.Equal(expectedQuantityAfterReverse, palletAfterReversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterReversePicking.Status);
			
			var pickingPalletAfterReverse = await DbContext.Pallets
				.Include(pp=>pp.ProductsOnPallet)
				.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);

			var reverseAfter = await DbContext.ReversePickings.ToListAsync();

			Assert.Contains(reverseAfter, x =>
				x.Id == task.Id &&
				x.Status == ReversePickingStatus.Completed);

			var history = DbContext.HistoryPickings;
			Assert.NotNull(history);
			Assert.Equal(4, history.Count());
			var history1 = DbContext.HistoryReversePickings;
			Assert.NotNull(history1);
			Assert.Equal(2, history1.Count());

		}

		[Fact]
		public async Task ExecuteReversePicking_ShouldRestoreProductToExistingPalletAndKeepPickingPalletActive_WhenAnotherProductStillRemains()
		{
			// Arrange
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var product1 = CreateProduct("Prod2", "SKU2");
			var location1 = CreateLocation(1, 1);
			var location2 = CreateLocation(2, 2);
			var location3 = CreateLocation(3, 3);
			var location4 = CreateLocation(4, 4);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			var pallet4 = Pallet.CreateForTests("P4", DateTime.UtcNow, 4, PalletStatus.Available, receipt.Id, null);
			pallet4.AddProduct(product1.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location1, locationBase, location2, location3, location4);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet4);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))  },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 4, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).Single();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.Single().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);
			Assert.Equal(PalletStatus.ToPicking, pallet4.Status);
			var pickingTaskToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTaskToDo);
			Assert.Equal(2, pickingTaskToDo.Count);

			//Act 2.1 - wykonanie pickingu - pomijamy strzelenie skanerem w paletę P2
			var pickingTaskForProduct = pickingTaskToDo.Single(p => p.ProductId == product.Id); 
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = pallet2.PalletNumber,
				ProductId = product.Id,
				RampNumber = 100100,

			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			
			//Assert 2.1
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(8, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);

			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);

			//Act 2.2 - wykonanie pickingu - pomijamy strzelenie skanerem w paletę P4
			var pickingTaskForProduct1 = pickingTaskToDo.FirstOrDefault(p => p.ProductId == product1.Id);
			var toPicking1 = new PickingTaskDTO
			{
				Id = pickingTaskForProduct1.Id,
				PickingStatus = pickingTaskForProduct1.PickingStatus,
				BestBefore = pickingTaskForProduct1.BestBefore,
				RequestedQuantity = pickingTaskForProduct1.RequestedQuantity,
				PickedQuantity = 4,
				SourcePalletId = pallet4.Id,
				SourcePalletNumber = "P4",
				ProductId = product1.Id,
				RampNumber = 100100,

			};
			var doPicking1 = new DoPlannedPickingCommand(toPicking1, "UserPicking1");
			var resultPicking1 = await Mediator.Send(doPicking1);
			//Assert 2.2
			Assert.True(resultPicking1.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct1.PickingStatus);
			Assert.Equal(4, pickingTaskForProduct1.PickedQuantity);
			var productOnPickingPallet1 = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product1.Id);
			Assert.NotNull(productOnPickingPallet1);
			Assert.Equal(4, productOnPickingPallet1.Quantity);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert 3
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);
			//Assert 3  reversePickingTask for product
			var reverseTasks = await DbContext.ReversePickings
				.ToListAsync();
			Assert.Equal(2, reverseTasks.Count);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet4.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Assert 3  reversePickingTask for product1
			var reverseTaskForProduct1 = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet4.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct1);

			var task1 = reverseTaskForProduct1.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task1.Status);
			Assert.Equal("UserC", task.UserId);
			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);

			var palletP4 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P4");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);			
			
			//ArrangeFor Act4
			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Pallets.Add(pallet3);
			await	DbContext.SaveChangesAsync();			
			var existingPallet =await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "P3");
			var list = new List<Pallet> { existingPallet};
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToExistingPallet,
				pickingPallet.Id, "UserReverse", list, null));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.IsSuccess);
			Assert.NotNull(resultReversePicking.Result);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar.", resultReversePicking.Result.Message);
			var palletAfterReversePicking = await DbContext.Pallets
				.Include(pp=>pp.ProductsOnPallet)
				.SingleAsync(p => p.PalletNumber == "P3");
			Assert.NotNull(palletAfterReversePicking);
			var listPalletIdsToAdd = resultReversePicking.Result.PalletWithAddedProduct
				.Select(x => x.PalletId)
				.ToList();
			Assert.Contains(palletAfterReversePicking.Id, listPalletIdsToAdd);
			Assert.NotNull(palletAfterReversePicking);
			const int existingQuantityOnTargetPallet = 1;
			const int pickedQuantityToReverse = 8;
			const int expectedQuantityAfterReverse = existingQuantityOnTargetPallet + pickedQuantityToReverse;
			Assert.Equal(expectedQuantityAfterReverse, palletAfterReversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterReversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets
				.Include(pp=>pp.ProductsOnPallet)
				.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity);
			Assert.Equal(4, pickingPalletAfterReverse.ProductsOnPallet.First(x => x.ProductId == product1.Id).Quantity);
			Assert.Equal(PalletStatus.ReversePicking, pickingPalletAfterReverse.Status);

			var reverseAfter = await DbContext.ReversePickings.ToListAsync();

			Assert.Contains(reverseAfter, x =>
				x.Id == task.Id &&
				x.Status == ReversePickingStatus.Completed);

			Assert.Contains(reverseAfter, x =>
				x.Id != task.Id &&
				x.Status == ReversePickingStatus.Ongoing);

			var history = DbContext.HistoryPickings;
			Assert.NotNull(history);
			Assert.Equal(4, history.Count());
			var history1 = DbContext.HistoryReversePickings;
			Assert.NotNull(history1);
			Assert.Equal(3, history1.Count());
		}
	}
}

//// Assert 3– pickingTasks cancelled
//var pickingTasksAfterCancelled = await DbContext.PickingTasks
//	.Where(a => a.IssueId == issue.Id)
//	.ToListAsync();

//Assert.Equal(2, pickingTasksAfterCancelled.Count);
//Assert.Equal(PickingStatus.Cancelled, pickingTasksAfterCancelled[0].PickingStatus);
//Assert.Equal(PickingStatus.Cancelled, pickingTasksAfterCancelled[1].PickingStatus);	