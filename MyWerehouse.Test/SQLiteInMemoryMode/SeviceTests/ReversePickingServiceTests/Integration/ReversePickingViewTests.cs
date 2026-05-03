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
using MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.ReversePickingServiceTests.Integration
{
	public class ReversePickingViewTests : TestBase
	{
		[Fact]
		public async Task GetReverseTasks_PalletsAsignmentAndPickingTaskDone_ShouldReturnList()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
			var location2 = new Location {  Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			//var location3 = new Location {  Aisle = 1, Bay = 1, Height = 1, Position = 3 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
			DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 100100);
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1, location2);
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletNumber = "P2",
				SourcePalletId = pallet2.Id,
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
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
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetListReversePickingToDoQuery(1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.Single(resultGetView.Result.Dtos);
		}
		[Fact]
		public async Task GetReturnReverseTask_PalletsAsignmentAndPickingTaskDone_ShouldReturnInfoOptionsToSourceAvailable()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location2 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
			DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 100100);
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1,location2);
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletNumber = "P2",
				SourcePalletId = pallet2.Id,
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
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
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.True(resultGetView.Result.CanReturnToSource);
			Assert.False(resultGetView.Result.CanAddToExistingPallet);
			Assert.False(resultGetView.Result.PickingPalletCompletlyUnpicking);
			Assert.True(resultGetView.Result.AddToNewPallet);
		}
		[Fact]
		public async Task GetReturnReverseTask_PalletsAsignmentAndPickingTaskDone_ShouldReturnInfoToExistOptionsAvailable()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location2 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			var location3 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 3 };
			var location4 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 4 };
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
			DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 100100);
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 8, new DateOnly(2026, 1, 1));
			
			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 7, new DateOnly(2026, 1, 1));
			
			var pallet4 = Pallet.CreateForTests("P4", DateTime.UtcNow, 4, PalletStatus.Available, receipt.Id, null);
			pallet4.AddProduct(product.Id, 5, new DateOnly(2026, 1, 1));
			
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1, location2, location3, location4);
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.RequestedQuantity ==5);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity =5, //8,
				SourcePalletId = pallet4.Id,
				SourcePalletNumber = "P4",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
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

		//	Assert.Single(pickingTasks);//Są dwa przez zmianę policy

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);

			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet4.Id)
				.ToListAsync();
			//jest jescze drugi pickingTask
			//Assert.Single(reverseTasks);

			var task = reverseTasks.FirstOrDefault(x=>x.Quantity ==5);
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.False(resultGetView.Result.CanReturnToSource);
			Assert.True(resultGetView.Result.CanAddToExistingPallet);
			Assert.True(resultGetView.Result.AddToNewPallet);
			Assert.True(resultGetView.Result.PickingPalletCompletlyUnpicking);
			Assert.Equal(2, resultGetView.Result.ListPalletsToAdd.Count);
		}
		[Fact]
		public async Task GetReturnReverseTask_PalletsAsignmentAndPickingTaskDone_ShouldReturnInfoOptionsReturnToNewOnly()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location2 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			var location3 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 3 };
			var location4 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 4 };
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
			DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 100100);
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			pallet2.AddProduct(product.Id, 8, new DateOnly(2026, 1, 1));

			var pallet3 = Pallet.CreateForTests("P3", DateTime.UtcNow, 3, PalletStatus.Available, receipt.Id, null);
			pallet3.AddProduct(product.Id, 7, new DateOnly(2026, 1, 1));

			//var pallet4 = Pallet.CreateForTests("P4", DateTime.UtcNow, 4, PalletStatus.Available, receipt.Id, null);
			//pallet4.AddProduct(product.Id, 5, new DateOnly(2026, 1, 1));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1, location2, location3, location4);
			DbContext.Pallets.AddRange(pallet1, pallet2
				, pallet3
				//, pallet4
				);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.RequestedQuantity == 7);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 7, //8,
				SourcePalletId = pallet3.Id,
				SourcePalletNumber = "P3",
				ProductId = product.Id,
				RampNumber = 100100,
			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);
			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
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

			//	Assert.Single(pickingTasks);//Są dwa przez zmianę policy

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);

			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == pallet3.Id)
				.ToListAsync();
			//jest jescze drugi pickingTask
			//Assert.Single(reverseTasks);

			var task = reverseTasks.FirstOrDefault(x => x.Quantity == 7);
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.False(resultGetView.Result.CanReturnToSource);
			Assert.True(resultGetView.Result.CanAddToExistingPallet);
			Assert.False(resultGetView.Result.PickingPalletCompletlyUnpicking);
			Assert.True(resultGetView.Result.AddToNewPallet);
			Assert.Equal(1, resultGetView.Result.ListPalletsToAdd.Count);
		}
	}
}
