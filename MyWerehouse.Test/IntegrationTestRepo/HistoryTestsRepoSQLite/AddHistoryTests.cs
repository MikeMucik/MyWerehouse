using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.HistoryTestsRepo
{
	public class AddHistoryTests : TestBase
	{
		[Fact]
		public void AddRecord_AddPalletMovement_AddToList()
		{
			//Arrange
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet1 = Pallet.CreateForTests("Q1000", DateTime.Now, 1, PalletStatus.Available, null, null);

			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.SaveChanges();
			var pallletMovement = new PalletMovement
			{
				PalletId = pallet1.Id,
				PalletNumber = pallet1.PalletNumber,
				SourceLocationId = location1.Id,
				DestinationLocationId = location2.Id,
				PalletMovementDetails = new List<PalletMovementDetail>
				{
					new PalletMovementDetail
					{
						Quantity = -1,
						ProductId =product.Id,
					}
				},
				PalletStatus = PalletStatus.Available,
				PerformedBy = "U001",
				Reason = ReasonMovement.Correction,
				MovementDate = DateTime.Now,
			};
			var palletMovementRepo = new PalletMovementRepo(DbContext);

			//Act
			palletMovementRepo.AddPalletMovement(pallletMovement);
			DbContext.SaveChanges();
			//Assert			
			var resultList = DbContext.PalletMovements.Where(m => m.PalletNumber == "Q1000");
			var result = resultList
				//.OrderByDescending(p => p.MovementDate)
				.FirstOrDefault();
			Assert.NotNull(result);
			Assert.Equal(-1, resultList.First(p => p.PerformedBy == "U001").PalletMovementDetails.First().Quantity);
			Assert.Equal("U001", result.PerformedBy);
			Assert.Equal(ReasonMovement.Correction, result.Reason);
		}
		[Fact]
		public void AddRecord_AddHistoryIssue_AddToList()
		{
			//Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet1 = Pallet.CreateForTests("Q1000", DateTime.Now, 1, PalletStatus.Available, null, null);

			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, DateTime.Now
				, DateTime.Now.AddDays(7), "user", IssueStatus.NotComplete, null);

			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var historyIssue = new HistoryIssue
			{
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ClientId = issue.ClientId,
				DateTime = DateTime.Now,
				//Items
				Details = new List<HistoryIssueDetail>
				{
					new HistoryIssueDetail
					{
						PalletId = pallet1.Id,
						PalletNumber = pallet1.PalletNumber,
						LocationId = location1.Id
					}
				},
				StatusAfter = IssueStatus.Archived,
				PerformedBy = "U001"
			};
			var historyIssueRepo = new HistoryIssueRepo(DbContext);
			//Act
			historyIssueRepo.AddHistoryIssue(historyIssue);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryIssues.Where(m => m.IssueId == issue.Id);
			var result = resultList.First();
			Assert.NotNull(result);
			Assert.Equal(IssueStatus.Archived, result.StatusAfter);
			Assert.Equal("U001", result.PerformedBy);
			Assert.Contains(result.Details, h => h.PalletNumber == "Q1000");
		}
		[Fact]
		public void AddRecord_AddHistoryReceipt_AddToList()
		{
			//Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet1 = Pallet.CreateForTests("Q1000", DateTime.Now, 1, PalletStatus.Available, null, null);

			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "User2",
			new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);

			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.Receipts.AddRange(receipt);
			DbContext.SaveChanges();
			var historyReceiptRepo = new HistoryReceiptRepo(DbContext);
			//Act
			var historyReceipt = new HistoryReceipt
			{

				ReceiptId = receipt.Id,
				ReceiptNumber = receipt.ReceiptNumber,
				ClientId = receipt.ClientId,
				StatusAfter = ReceiptStatus.Verified,
				DateTime = DateTime.Now,
				PerformedBy = receipt.PerformedBy,
			};
			historyReceiptRepo.AddHistoryReceipt(historyReceipt);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryReceipts.Where(m => m.ReceiptNumber == receipt.ReceiptNumber);
			var result = resultList.First();
			Assert.NotNull(result);
			Assert.Equal(ReceiptStatus.Verified, result.StatusAfter);
			Assert.Equal("User2", result.PerformedBy);
			//Assert.Contains(result.Details, h => h.PalletId == "Q1000");
		}
		[Fact]
		public async Task AddRecord_AddHistoryPicking_AddToList()
		{
			//Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, DateTime.Now
			, DateTime.Now.AddDays(7), "user", IssueStatus.NotComplete, null);

			var pallet1 = Pallet.CreateForTests("Q1000", DateTime.Now, 1, PalletStatus.Available, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var virtualPallet = VirtualPallet.Create(pallet1.Id, 100, location1.Id);
			var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 10, PickingStatus.Allocated, product.Id,
					null, null, null, 0);
			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			var historyPicking = new HistoryPicking
			{
				PickingTaskId = virtualPallet.PickingTasks.First().Id,
				//PickingTaskNumber = virtualPallet.PickingTasks.First().PickingTaskNumber,
				QuantityAllocated = virtualPallet.PickingTasks.First().RequestedQuantity,
				StatusAfter = PickingStatus.Picked,
				DateTime = DateTime.Now,
				PerformedBy = "A",
				StatusBefore = PickingStatus.Allocated,
				ProductId = virtualPallet.PickingTasks.First().ProductId,
				QuantityPicked = 1,
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				PalletId = pallet1.Id,
				PalletNumber = pallet1.PalletNumber

			};
			var historyPickingRepo = new HistoryPickingRepo(DbContext);
			//Act
			historyPickingRepo.AddHistoryPicking(historyPicking);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryPickings.Where(m => m.PalletId == virtualPallet.PalletId);
			var result1 = await resultList.FirstAsync();
			var result = resultList.First();
			Assert.NotNull(result);
			Assert.Equal(PickingStatus.Picked, result.StatusAfter);
			Assert.Equal("A", result.PerformedBy);
		}
		[Fact]
		public async Task AddRecord_AddHistoryReversePicking_AddToList()
		{
			//Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, DateTime.Now
			, DateTime.Now.AddDays(7), "user", IssueStatus.NotComplete, null);

			var pallet1 = Pallet.CreateForTests("Q1000", DateTime.Now, 1, PalletStatus.Available, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var virtualPallet = VirtualPallet.Create(pallet1.Id, 100, location1.Id);

			var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 10, PickingStatus.Allocated, product.Id,
					null, null, null, 0);

			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.SaveChanges();
			//wykonaj picking i utwórz paletę
			var pickingPallet = Pallet.CreateForTests("Q1001", DateTime.Now, 1, PalletStatus.ToIssue, null, issue.Id);
			pickingPallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)));
			//DbContext.Clients.Add(initailClient);
			var reverseTask = new ReversePicking
			{
				Id = Guid.NewGuid(),
				PickingPalletId = pickingPallet.Id,
				SourcePalletId = pallet1.Id,
				ProductId = product.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)),
				Quantity = 10,
				Status = ReversePickingStatus.Pending,
				PickingTaskId = pickingTask.Id,
				DateMade = DateOnly.FromDateTime(DateTime.UtcNow),
				UserId = "UserReserve"

			};

			var historyReversePicking = new HistoryReversePicking
			{
				ReversePickingId = reverseTask.Id,
				PickingPalletId = pickingPallet.Id,
				PickingPalletNumber = pickingPallet.PalletNumber,
				PalletSourceId = reverseTask.SourcePalletId,
				PalletSourceNumber = pallet1.PalletNumber,
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ProductId = reverseTask.ProductId,
				DateTime = DateTime.UtcNow,
				PerformedBy = reverseTask.UserId,
				Quantity = reverseTask.Quantity,
				//StatusBefore = notification.StatusBefore,
				StatusAfter =ReversePickingStatus.Pending
			};

			var historyReversePickingRepo = new HistoryReversePickingRepo(DbContext);
			var ct = CancellationToken.None;
			//Act
			await historyReversePickingRepo.AddHistoryReversePickingAsync(historyReversePicking, ct);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryReversePickings.Where(m => m.PickingPalletId == pickingPallet.Id);
			var result1 = await resultList.FirstAsync();
			var result = resultList.First();
			Assert.NotNull(result1);
			Assert.Equal(ReversePickingStatus.Pending, result1.StatusAfter);
			Assert.Equal(reverseTask.UserId, result1.PerformedBy);
		}
	}
}

