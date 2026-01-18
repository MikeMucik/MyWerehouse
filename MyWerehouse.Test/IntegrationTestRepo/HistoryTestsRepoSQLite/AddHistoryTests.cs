using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Repositories;
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
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
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
			var pallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.SaveChanges();
			var pallletMovement = new PalletMovement
			{
				PalletId = pallet1.Id,
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
			var resultList = DbContext.PalletMovements.Where(m => m.PalletId == "Q1000");
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
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
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
			var pallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var issue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = DateTime.Now,
				Pallets = new List<Pallet> { pallet1 },
				//IssueStatus = IssueStatus.NotComplete,
				PerformedBy = "user",
				//IssueDateTimeSend = DateTime.Now,	
			};
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
				ClientId = issue.ClientId,
				DateTime = DateTime.Now,
				//Items
				Details = new List<HistoryIssueDetail>
				{
					new HistoryIssueDetail
					{
						PalletId = pallet1.Id,
						LocationId = location1.Id
					}
				},
				StatusAfter = IssueStatus.Archived,
				PerformedBy = "U001"
			};
			var historyIssueRepo = new Infrastructure.Repositories. HistoryIssueRepo(DbContext);
			//Act
			historyIssueRepo.AddHistoryIssue(historyIssue);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryIssues.Where(m => m.IssueId == issue.Id);
			var result = resultList.First();
			Assert.NotNull(result);
			Assert.Equal(IssueStatus.Archived, result.StatusAfter);
			Assert.Equal("U001", result.PerformedBy);
			Assert.Contains(result.Details, h => h.PalletId == "Q1000");
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
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};

			var pallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var receipt = new Receipt
			{
				Client = initailClient,
				Pallets = new List<Pallet> { pallet1 },
				ReceiptDateTime = DateTime.Now,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "User2"
			};
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
				ClientId = receipt.ClientId,
				StatusAfter = ReceiptStatus.Verified,
				DateTime = DateTime.Now,
				PerformedBy = receipt.PerformedBy,
			};
			historyReceiptRepo.AddHistoryReceipt(historyReceipt);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryReceipts.Where(m => m.ReceiptId == receipt.Id);
			var result = resultList.First();
			Assert.NotNull(result);
			Assert.Equal(ReceiptStatus.Verified, result.StatusAfter);
			Assert.Equal("User2", result.PerformedBy);
			//Assert.Contains(result.Details, h => h.PalletId == "Q1000");
		}
		[Fact]
		public void AddRecord_AddHistoryPicking_AddToList()
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
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
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
			var pallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet> { new ProductOnPallet
				{
					Product= product,
					Quantity = 10,
					
				} }
				};
			var issue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = DateTime.Now,
				Pallets = new List<Pallet> { pallet1 },
				//IssueStatus = IssueStatus.NotComplete,
				PerformedBy = "user",
				//IssueDateTimeSend = DateTime.Now,	
			};
			var virtualPallet = new VirtualPallet
			{
				Location = location1,
				Pallet = pallet1,
				InitialPalletQuantity = 100,
				DateMoved = DateTime.Now,
				Allocations = new List<Allocation> {new Allocation
					{
						Issue = issue,
						PickingStatus = PickingStatus.Allocated,
						Quantity = 10,
					}
				}
			};

			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1);
			DbContext.Issues.AddRange(issue);
			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.SaveChanges();
			var historyPicking = new HistoryPicking
			{
				AllocationId = virtualPallet.Allocations.First().Id,
				QuantityAllocated = virtualPallet.Allocations.First().Quantity,
				StatusAfter = PickingStatus.Picked,
				DateTime = DateTime.Now,
				PerformedBy = "A",
				StatusBefore = PickingStatus.Allocated,
				ProductId = virtualPallet.Allocations.First().ProductId,
				QuantityPicked = 1,
				IssueId = issue.Id,
				PalletId = pallet1.Id,

			};
			var historyPickingRepo = new HistoryPickingRepo(DbContext);
			//Act
			historyPickingRepo.AddHistoryPicking(historyPicking);
			DbContext.SaveChanges();
			//Assert
			var resultList = DbContext.HistoryPickings.Where(m => m.Id == virtualPallet.Id);
			var result = resultList.First();
			Assert.NotNull(result);
			Assert.Equal(PickingStatus.Picked, result.StatusAfter);
			Assert.Equal("A", result.PerformedBy);
		}
		[Fact]
		public async Task AddRecord_AddHistoryReversePicking_AddToList()
		{
			//Arrange
			var historyReversePicking = new HistoryReversePicking
			{

			};
			var historyReversePickingRepo = new HistoryReversePickingRepo(DbContext);
			//Act
		//await	historyReversePickingRepo.AddHistoryReversePickingAsync(historyReversePicking, cancellationToken);
		}
	}
}

