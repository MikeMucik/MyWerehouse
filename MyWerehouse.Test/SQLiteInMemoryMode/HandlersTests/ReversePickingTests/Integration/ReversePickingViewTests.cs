using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Picking.Commands.DoPlannedPicking;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking;
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
	public class ReversePickingViewTests : TestBase
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
			return Product.Create(name, sku, 1, 10);
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
		private static Receipt CreateReceipt()
		{
			return Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMake", DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 100100);
		}
		[Fact]
		public async Task GetListReversePickingToDo_ShouldReturnList_WhenOneTaskDone()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location = CreateLocation(1, 1);
			var location1 = CreateLocation(2, 2);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, locationBase, location1);
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).Single();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.Single().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);
			var pickingTaskToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTaskToDo);
			Assert.Single(pickingTaskToDo);
			//Act 2 - wykonanie pickingu
			var pickingTaskForProduct = DbContext.PickingTasks.Single(x => x.IssueId == issue.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletNumber = "P2",
				SourcePalletId = pallet2.Id,
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert
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
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);

			// Assert – Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetListReversePickingToDoQuery(1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.NotNull(resultGetView.Result);
			Assert.Single(resultGetView.Result.Items);
		}
		[Fact]
		public async Task GetListReversePickingToDo_ShouldReturnList_WhenTwoPickingTaskDone()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location = CreateLocation(1, 1);
			var location1 = CreateLocation(2, 2);
			var location2 = CreateLocation(3, 3);
			var location3 = CreateLocation(4, 4);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 7, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, locationBase, location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);//bo od najmniejszej ilości
			Assert.Equal(PalletStatus.ToPicking, pallet3.Status);//bo od najmniejszej ilości
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Equal(2, pickingTasksToDo.Count);
			//Act 2.1 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 7);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 7, //8,
				SourcePalletId = pallet3.Id,
				SourcePalletNumber = "P3",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2.1
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(7, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);
			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(7, productOnPickingPallet.Quantity);

			//Act 2.2 - wykonanie pickingu
			var pickingTaskForProductSecond = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 1);
			var toPickingSecond = new PickingTaskDTO
			{
				Id = pickingTaskForProductSecond.Id,
				PickingStatus = pickingTaskForProductSecond.PickingStatus,
				BestBefore = pickingTaskForProductSecond.BestBefore,
				RequestedQuantity = pickingTaskForProductSecond.RequestedQuantity,
				PickedQuantity = 1, //8,
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = "P2",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPickingSecond = new DoPlannedPickingCommand(toPickingSecond, "UserPicking");
			var resultPickingSecond = await Mediator.Send(doPickingSecond);
			//Assert 2.2
			Assert.True(resultPickingSecond.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProductSecond.PickingStatus);
			Assert.Equal(1, pickingTaskForProductSecond.PickedQuantity);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));

			//Assert
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
			Assert.Equal(2, reverseTasks.Count);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet3.Id);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet3.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);

			var reverseTaskForProductSecond = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProductSecond);

			var taskSecond = reverseTaskForProductSecond.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP3 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P3");
			Assert.Equal(PalletStatus.Archived, palletP3.Status);
			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.Archived, palletP2.Status);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetListReversePickingToDoQuery(10, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.True(resultGetView.IsSuccess);
			Assert.NotNull(resultGetView.Result);
			Assert.Equal(2, resultGetView.Result.Items.Count);
		}		
		[Fact]
		public async Task GetReturnReverseTask_ShouldReturnInfoOptionsToSourceAvailable()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location = CreateLocation(1, 1);
			var location1 = CreateLocation(2, 2);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, locationBase, location1);
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));

			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).Single();
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
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.True(resultGetView.IsSuccess);
			Assert.NotNull(resultGetView.Result);
			Assert.True(resultGetView.Result.CanReturnToSource);
			Assert.False(resultGetView.Result.CanAddToExistingPallet);
			Assert.False(resultGetView.Result.PickingPalletCompletlyUnpicking);
			Assert.True(resultGetView.Result.AddToNewPallet);
		}
		[Fact]
		public async Task GetReturnReverseTask_ShouldReturnInfoToExistOptionsAvailable()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location = CreateLocation(1, 1);
			var location1 = CreateLocation(2, 2);
			var location2 = CreateLocation(3, 3);
			var location3 = CreateLocation(4, 4);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 8, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 7, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet4 = Pallet.CreateForTests("P4", DateTime.UtcNow, 4, PalletStatus.Available, receipt.Id, null);
			pallet4.AddProduct(product.Id, 5, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, locationBase, location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet3.Status);//bo od najmniejszej ilości stąd 3
			Assert.Equal(PalletStatus.ToPicking, pallet4.Status);//bo od najmniejszej ilości stąd 5
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Equal(2, pickingTasksToDo.Count);
			//Act 2 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 5);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 5,
				SourcePalletId = pallet4.Id,
				SourcePalletNumber = "P4",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(5, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);
			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(5, productOnPickingPallet.Quantity);
						
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
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
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet4.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet4.Id)
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

			var palletP4 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P4");
			Assert.Equal(PalletStatus.Archived, palletP4.Status);
			// Assert 3– picingTask
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);

			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.True(resultGetView.IsSuccess);
			Assert.NotNull(resultGetView.Result);
			Assert.False(resultGetView.Result.CanReturnToSource);
			Assert.True(resultGetView.Result.CanAddToExistingPallet);
			Assert.True(resultGetView.Result.AddToNewPallet);
			Assert.True(resultGetView.Result.PickingPalletCompletlyUnpicking);
			Assert.Equal(2, resultGetView.Result.ListPalletsToAdd.Count);
		}
		[Fact]
		public async Task GetReturnReverseTask_ShouldReturnInfoOptionsReturnToNewOnly()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location = CreateLocation(1, 1);
			var location1 = CreateLocation(2, 2);
			var location2 = CreateLocation(3, 3);
			var location3 = CreateLocation(4, 4);
			var locationBase = CreateLocation(100100, 5);
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 7, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, locationBase, location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);//bo od najmniejszej ilości
			Assert.Equal(PalletStatus.ToPicking, pallet3.Status);//bo od najmniejszej ilości
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Equal(2, pickingTasksToDo.Count);
			//Act 2.1 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 7);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 7, //8,
				SourcePalletId = pallet3.Id,
				SourcePalletNumber = "P3",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2.1
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(7, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);
			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(7, productOnPickingPallet.Quantity);

			//Act 2.2 - wykonanie pickingu
			var pickingTaskForProductSecond = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 1);
			var toPickingSecond = new PickingTaskDTO
			{
				Id = pickingTaskForProductSecond.Id,
				PickingStatus = pickingTaskForProductSecond.PickingStatus,
				BestBefore = pickingTaskForProductSecond.BestBefore,
				RequestedQuantity = pickingTaskForProductSecond.RequestedQuantity,
				PickedQuantity = 1, //8,
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = "P2",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPickingSecond = new DoPlannedPickingCommand(toPickingSecond, "UserPicking");
			var resultPickingSecond = await Mediator.Send(doPickingSecond);
			//Assert 2.2
			Assert.True(resultPickingSecond.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProductSecond.PickingStatus);
			Assert.Equal(1, pickingTaskForProductSecond.PickedQuantity);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));

			//Assert
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
			Assert.Equal(2, reverseTasks.Count);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet3.Id);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet3.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);

			var reverseTaskForProductSecond = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProductSecond);

			var taskSecond = reverseTaskForProductSecond.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP3 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P3");
			Assert.Equal(PalletStatus.Archived, palletP3.Status);
			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.Archived, palletP2.Status);
			
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.True(resultGetView.IsSuccess);
			Assert.NotNull(resultGetView.Result);
			Assert.False(resultGetView.Result.CanReturnToSource);
			Assert.False(resultGetView.Result.CanAddToExistingPallet);
			Assert.False(resultGetView.Result.PickingPalletCompletlyUnpicking);
			Assert.True(resultGetView.Result.AddToNewPallet);
			Assert.Empty(resultGetView.Result.ListPalletsToAdd);
		}

		[Fact]
		public async Task ListPalletsForForkLifterReservePicking_ReturnList()
		{
			// Arrange 
			var client = CreateClient();
			var category = CreateCategory("Cat");
			var product = CreateProduct("Prod1", "123456");
			var location = CreateLocation(1, 1);
			var location1 = CreateLocation(2, 2);
			var location2 = CreateLocation(3, 3);
			var location3 = CreateLocation(4, 4);
			var locationBase = CreateLocation(100100, 6);
			var receipt = CreateReceipt();
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 7, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, locationBase, location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))));
			//Assert 1
			Assert.True(created.IsSuccess);
			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			Assert.Equal(PalletStatus.ToPicking, pallet2.Status);//bo od najmniejszej ilości
			Assert.Equal(PalletStatus.ToPicking, pallet3.Status);//bo od najmniejszej ilości
			var pickingTasksToDo = await DbContext.PickingTasks.Where(x => x.IssueId == issue.Id).ToListAsync();
			Assert.NotEmpty(pickingTasksToDo);
			Assert.Equal(2, pickingTasksToDo.Count);
			//Act 2.1 - wykonanie pickingu
			var pickingTaskForProduct = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 7);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingTaskForProduct.Id,
				PickingStatus = pickingTaskForProduct.PickingStatus,
				BestBefore = pickingTaskForProduct.BestBefore,
				RequestedQuantity = pickingTaskForProduct.RequestedQuantity,
				PickedQuantity = 7, 
				SourcePalletId = pallet3.Id,
				SourcePalletNumber = "P3",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			//Assert 2.1
			Assert.True(resultPicking.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProduct.PickingStatus);
			Assert.Equal(7, pickingTaskForProduct.PickedQuantity);
			var pickingPallet = await DbContext.Pallets.SingleAsync(x => x.PalletNumber == "Q0001");
			Assert.NotNull(pickingPallet);
			var productOnPickingPallet = pickingPallet.ProductsOnPallet
			.SingleOrDefault(p => p.ProductId == product.Id);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(7, productOnPickingPallet.Quantity);

			//Act 2.2 - wykonanie pickingu
			var pickingTaskForProductSecond = pickingTasksToDo.Single(x => x.IssueId == issue.Id && x.RequestedQuantity == 1);
			var toPickingSecond = new PickingTaskDTO
			{
				Id = pickingTaskForProductSecond.Id,
				PickingStatus = pickingTaskForProductSecond.PickingStatus,
				BestBefore = pickingTaskForProductSecond.BestBefore,
				RequestedQuantity = pickingTaskForProductSecond.RequestedQuantity,
				PickedQuantity = 1, 
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = "P2",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var doPickingSecond = new DoPlannedPickingCommand(toPickingSecond, "UserPicking");
			var resultPickingSecond = await Mediator.Send(doPickingSecond);
			//Assert 2.2
			Assert.True(resultPickingSecond.IsSuccess);
			Assert.Equal(PickingStatus.Picked, pickingTaskForProductSecond.PickingStatus);
			Assert.Equal(1, pickingTaskForProductSecond.PickedQuantity);

			Assert.NotNull(productOnPickingPallet);
			Assert.Equal(8, productOnPickingPallet.Quantity);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));

			//Assert
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
			Assert.Equal(2, reverseTasks.Count);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet3.Id);
			Assert.Contains(reverseTasks, x => x.SourcePalletId == pallet2.Id);

			var reverseTaskForProduct = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet3.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProduct);

			var task = reverseTaskForProduct.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);

			var reverseTaskForProductSecond = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet2.Id)
				.ToListAsync();

			Assert.Single(reverseTaskForProductSecond);

			var taskSecond = reverseTaskForProductSecond.Single();
			Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Ongoing, task.Status);
			Assert.Equal("UserC", task.UserId);
			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP3 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P3");
			Assert.Equal(PalletStatus.Archived, palletP3.Status);
			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.Archived, palletP2.Status);
			//Arrange for 4
			var today = DateOnly.FromDateTime(DateTime.UtcNow);
			var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
			//Act 4
			var listForOperator = await Mediator.Send(new ListPalletsForForkLifterReservePickingQuery(today, tomorrow));
			//Assert
			Assert.NotNull(listForOperator);
			Assert.True(listForOperator.IsSuccess);
			Assert.NotNull(listForOperator.Result);
			Assert.NotEmpty(listForOperator.Result);
			Assert.Single(listForOperator.Result);
			var pickingPalletToTakeDown = listForOperator.Result.Single();
			Assert.Equal(100100, pickingPalletToTakeDown.LocationId);
			Assert.Equal("1-1-6-1", pickingPalletToTakeDown.LocationName);
		}
	}
}
