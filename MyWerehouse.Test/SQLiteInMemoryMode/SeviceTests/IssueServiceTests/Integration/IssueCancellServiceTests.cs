using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueCancellServiceTests : TestBase
	{
		[Fact]
		public async Task CancelIssueAsync_FullPalletAsignment_ShouldRestorePalletAvailability()
		{
			// Arrange – setup initial data
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
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location1 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
				DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 1);
			var palletP1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			palletP1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			var palletP2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			palletP2.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(palletP1, palletP2);
			DbContext.Receipts.Add(receipt);
			await DbContext.SaveChangesAsync();
			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};
			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			// Act 2 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			Assert.NotNull(result);
			var pallet = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P1");

			Assert.Equal(PalletStatus.Available, pallet.Status);
		}
		[Fact]
		public async Task CancelIssueAsync_FullPalletsAsignment_ShouldRestorePalletsAvailability()
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
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location1 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
			DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 1);			
			var palletP1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			palletP1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			var palletP2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 2, PalletStatus.Available, receipt.Id, null);
			palletP2.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);

			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(palletP1, palletP2);
			await DbContext.SaveChangesAsync();			
			await DbContext.SaveChangesAsync();
			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 20, BestBefore = new DateOnly(2026,1,1) }
				}
			};
			var created = await Mediator.Send(new CreateIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			//Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			// Act 2 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			Assert.NotNull(result);
			var pallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.PalletNumber == "P1");
			var pallet1 = await DbContext.Pallets.FirstOrDefaultAsync(p => p.PalletNumber == "P2");

			Assert.Equal(PalletStatus.Available, pallet.Status);
			Assert.Equal(PalletStatus.Available, pallet1.Status);
		}
		[Fact]
		public async Task CancelIssueAsync_PalletsAsignmentAndPickingTask_ShouldRestorePalletsAvailability()
		{
			// Arrange – setup initial data
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
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location1 = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 2 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
				DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 1);
			var palletPP1 = Pallet.CreateForTests("P1", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			palletPP1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			var palletPP2 = Pallet.CreateForTests("P2", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			palletPP2.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(palletPP1, palletPP2);
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
			Assert.Single(issue.PickingTasks);
			Assert.Equal(PalletStatus.LockedForIssue, issue.Pallets.First().Status);
			// Act 2 - cancel issue
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

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.PalletNumber == "P2");
			Assert.Equal(PalletStatus.Available, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(1, palletP2.LocationId);

			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Empty(pickingTasks);

			var historyPicking = await DbContext.HistoryPickings.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.PerformedBy == "UserC");
			Assert.Equal(PickingStatus.Cancelled, historyPicking.StatusAfter);
			Assert.Equal(PickingStatus.Allocated, historyPicking.StatusBefore);

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);

		}
		[Fact]
		public async Task CancelIssueAsync_PalletsAsignmentAndPickingTaskDone_ShouldRestorePalletsAvailabilityAndAddReversePicking()
		{
			// Arrange – setup initial data
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
			var category = new Category { Id = 1, Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var location2 = new Location { Aisle = 1, Bay = 1, Height = 2, Position = 1 };
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
			var product = Product.Create("Prod1", "SKU1", 1, 10);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, 1, "UserMakae",
				DateTime.UtcNow.AddDays(-1), ReceiptStatus.Verified, 1);
			var pallet1 = Pallet.CreateForTests("P1", DateTime.UtcNow.AddDays(-10), 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			var pallet2 = Pallet.CreateForTests("P2", DateTime.UtcNow.AddDays(-9), 2, PalletStatus.Available, receipt.Id, null);
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
			Assert.Equal(issue.Pallets.First().PalletNumber, "P1");
			//Assert.Equal(issue.Pallets.First().PalletNumber, "P1");
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
				SourcePalletId = pallet2.Id,
				SourcePalletNumber = "P2",
				ProductId = product.Id,
				RampNumber = 100100
			};
			var doPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(doPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);
			Assert.Equal(2, issue.Pallets.Count);
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

		}
	}
}
