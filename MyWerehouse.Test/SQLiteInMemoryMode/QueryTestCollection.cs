using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.SQLiteInMemoryMode
{
	[CollectionDefinition("QueryCollection")]
	public class QueryTestCollection : ICollectionFixture<QueryTestFixture> { }

	public class QueryTestFixture : TestBase  // Dziedziczy po TestBase (SQLite in-memory)
	{
		public QueryTestFixture()
			: base()  // Wywołuje ctor TestBase (connection, options, EnsureCreated)
		{
			SeedDatabase(DbContext);  // Seeduj dane 
		}
		public WerehouseDbContext DbContext => base.DbContext;
		// Opcjonalnie: Metoda do tworzenia czystego kontekstu (bez seedu, dla izolowanych testów)
		public WerehouseDbContext CreateCleanContext() => CreateNewContext();  // Z TestBase
		public static void SeedDatabase(WerehouseDbContext context)
		{
			// 1. Dane podstawowe
			if (!context.Clients.Any())
			{
				context.Clients.AddRange(
					new Client { Id = 10, Name = "ClientTest", Email = "client@op.pl", Description = "ClientDescription", FullName = "FullNameTestAddress" },
					new Client { Id = 11, Name = "ClientTest1", Email = "client1@op.pl", Description = "ClientDescription1", FullName = "FullNameTestAddress1" },
					new Client { Id = 989, Name = "ClientTest2", Email = "client2@op.pl", Description = "ClientDescription2", FullName = "FullNameTestAddress2" }
				);
			}

			if (!context.Users.Any())
			{
				context.Users.AddRange(
					new IdentityUser { Id = "TestUser", UserName = "TestUser" },
					new IdentityUser { Id = "U002", UserName = "TestUser2" },
					new IdentityUser { Id = "UserR", UserName = "TestUserRR" }

				);
			}

			if (!context.Categories.Any())
			{
				context.Categories.AddRange(
					new Category { Id = 1, Name = "TestCategory", IsDeleted = false },
					new Category { Id = 2, Name = "TestCategory1", IsDeleted = false },
					new Category { Id = 3, Name = "ToDeleted", IsDeleted = false }
				);
			}

			var location1 = new Location { Id = 1, Aisle = 1, Bay = 2, Position = 3, Height = 4 };
			var location2 = new Location { Id = 2, Aisle = 2, Bay = 3, Position = 4, Height = 5 };
			var location3 = new Location { Id = 3, Aisle = 24, Bay = 3, Position = 4, Height = 5 };
			var location4 = new Location { Id = 20, Aisle = 3, Bay = 3, Position = 4, Height = 5 };
			if (!context.Locations.Any())
			{

				context.Locations.AddRange(location1, location2, location3, location4);
			}

			context.SaveChanges();

			// 2. Dane zależne od powyższych
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");
			var productId989 = Guid.Parse("00000000-0000-0000-0989-000000000000");
			if (!context.Products.Any())
			{
				context.Products.AddRange(Product.CreateForSeed(productId1, "Test", "0987654321", new DateTime(2025, 05, 01), 1, false, 56),
					Product.CreateForSeed(productId2, "TestD", "fghtredfg", new DateTime(2025, 05, 01), 1, false, 44),
					Product.CreateForSeed(productId989, "NotAdded", "fghtredfg", new DateTime(2025, 05, 01), 1, false, 112)

				);
			}

			if (!context.ProductDetails.Any())
			{
				context.ProductDetails.AddRange(
					ProductDetail.CreateDetails(productId1, 10, 20, 30, 2, "TestDetails"),
					ProductDetail.CreateDetails(productId2, 20, 40, 60, 3, "TestDetails 11"));
			}

			if (!context.Addresses.Any())
			{
				context.Addresses.AddRange(
					new Address { Id = 10, Country = "ConutryTest", City = "CityTest", Region = "RegionTest", Phone = 123123123, PostalCode = "12ggt", StreetName = "StreetTest", StreetNumber = "12/1", ClientId = 10 },
					new Address { Id = 11, Country = "ConutryTest1", City = "CityTest1", Region = "RegionTest1", Phone = 987987987, PostalCode = "test12ggt", StreetName = "StreetTest1", StreetNumber = "12/11", ClientId = 10 }
				);
			}
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receiptId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			if (!context.Receipts.Any())
			{
				context.Receipts.AddRange(
					Receipt.CreateForSeed(receiptId1, 1, 10, "U001", new DateTime(2023, 3, 3), ReceiptStatus.Verified, 1),
					Receipt.CreateForSeed(receiptId2, 2, 11, "U002", new DateTime(2023, 4, 4), ReceiptStatus.Verified, 1)
														);
			}
			var issueId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			if (!context.Issues.Any())
			{
				var issueItems = new List<IssueItem>
				{
					IssueItem.CreateForSeed(1,issueId2, productId1, 150, DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),  DateTime.Today),
					IssueItem.CreateForSeed(2,issueId2, productId2, 400, DateOnly.FromDateTime(DateTime.Today.AddMonths(3)), DateTime.Today)
				};
				context.Issues.Add(
					Issue.CreateForSeed(issueId2, 2, 11, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddHours(23), "U002", IssueStatus.New, issueItems));
			}

			context.SaveChanges();

			// 3. Dane końcowe, zależne od receipt/issue/product

			var palletGuid1 = Guid.Parse("00000000-0001-1111-0000-000000000000");
			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");
			var palletGuid3 = Guid.Parse("00000000-0003-1111-0000-000000000000");
			var palletGuid4 = Guid.Parse("00000000-0004-1111-0000-000000000000");
			var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");
			var palletGuid6 = Guid.Parse("00000000-0006-1111-0000-000000000000");
			var palletGuid7 = Guid.Parse("00000000-0007-1111-0000-000000000000");
			var palletGuid8 = Guid.Parse("00000000-0008-1111-0000-000000000000");
			var palletGuid9 = Guid.Parse("00000000-0009-1111-0000-000000000000");


			if (!context.Pallets.Any())
			{
				context.Pallets.AddRange(
					Pallet.CreateForSeed(palletGuid1, "Q1000", new DateTime(2020, 1, 1), 1, PalletStatus.Available, receiptId1, issueId2),
					Pallet.CreateForSeed(palletGuid2, "Q1001", new DateTime(2020, 1, 1), 1, PalletStatus.OnHold, receiptId1, issueId2),
					Pallet.CreateForSeed(palletGuid3, "Q1002", new DateTime(2020, 1, 1), 3, PalletStatus.Available, receiptId2, null),
					Pallet.CreateForSeed(palletGuid4, "Q1010", new DateTime(2025, 1, 1), 3, PalletStatus.Damaged, receiptId2, null),
					Pallet.CreateForSeed(palletGuid5, "Q1100", new DateTime(2025, 1, 1), 3, PalletStatus.ToPicking, receiptId2, null),
					Pallet.CreateForSeed(palletGuid6, "Q1101", new DateTime(2025, 1, 5), 3, PalletStatus.ToPicking, receiptId2, null),
					Pallet.CreateForSeed(palletGuid7, "Q2000", new DateTime(2025, 1, 1), 3, PalletStatus.ToIssue, receiptId2, issueId2),
					Pallet.CreateForSeed(palletGuid8, "Q1200", new DateTime(2025, 2, 1), 3, PalletStatus.ToPicking, receiptId2, null),
					//PickingPallet
					Pallet.CreateForSeed(palletGuid9, "Q5000", new DateTime(2025, 2, 1), 3, PalletStatus.ToPicking, null, null)
				);
			}
			var vpId1 = Guid.Parse("22222222-1111-2222-1111-111111111111");
			var vpId2 = Guid.Parse("22222222-2222-2222-1111-111111111111");
			var vpId3 = Guid.Parse("22222222-3333-2222-1111-111111111111");
			if (!context.VirtualPallets.Any())
			{
				context.VirtualPallets.AddRange(
					VirtualPallet.CreateForSeed(vpId1, palletGuid5, 200, 3, DateTime.UtcNow.AddDays(-1)),
					VirtualPallet.CreateForSeed(vpId2, palletGuid6, 150, 3, new DateTime(2024, 6, 6)),
					VirtualPallet.CreateForSeed(vpId3, palletGuid8, 300, 3, DateTime.UtcNow.AddDays(-1)));					
			}
			var pickingId1 = Guid.Parse("11111111-1111-2222-1111-111111111111");
			var pickingId2 = Guid.Parse("11111111-2222-2222-1111-111111111111");
			var pickingId3 = Guid.Parse("11111111-3333-2222-1111-111111111111");
			var pickingId4 = Guid.Parse("11111111-4444-2222-1111-111111111111");
			var pickingId5 = Guid.Parse("11111111-5555-2222-1111-111111111111");
			var pickingId6 = Guid.Parse("11111111-6666-2222-1111-111111111111");
			if (!context.PickingTasks.Any())
			{
				context.PickingTasks.AddRange(
					PickingTask.CreateForSeed(pickingId1, vpId1, issueId2, 20, PickingStatus.Allocated, productId2,
				DateOnly.FromDateTime(DateTime.Today.AddDays(366)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2)), 0),				
				PickingTask.CreateForSeed(pickingId2, vpId1, issueId2, 20, PickingStatus.Picked, productId2,
				DateOnly.FromDateTime(DateTime.Today.AddDays(366)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2)), 20),				
				PickingTask.CreateForSeed(pickingId3, vpId2, issueId2, 50, PickingStatus.Allocated, productId2,
				DateOnly.FromDateTime(DateTime.Today.AddDays(366)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2)), 0),				
				PickingTask.CreateForSeed(pickingId4, vpId3, issueId2, 100, PickingStatus.Allocated, productId1,
				DateOnly.FromDateTime(DateTime.Today.AddDays(366)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2)), 0),				
				PickingTask.CreateForSeed(pickingId5, vpId3, issueId2, 10, PickingStatus.Picked, productId1,
				DateOnly.FromDateTime(DateTime.Today.AddDays(366)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2)), 10),				
				PickingTask.CreateForSeed(pickingId6, vpId1, issueId2, 20, PickingStatus.Allocated, productId2,
				DateOnly.FromDateTime(DateTime.Today.AddDays(366)), null, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2)), 10)					
					);
			}
			if (!context.ProductOnPallet.Any())
			{
				context.ProductOnPallet.AddRange(
					ProductOnPallet.CreateForSeed(1, productId1, palletGuid1, 50, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(2, productId1, palletGuid2, 100, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(3, productId2, palletGuid1, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(4, productId2, palletGuid4, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(5, productId2, palletGuid3, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(6, productId2, palletGuid5, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(7, productId2, palletGuid7, 200, new DateTime(2024, 2, 2), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(8, productId2, palletGuid6, 150, new DateTime(2024, 3, 3), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(9, productId1, palletGuid8, 300, new DateTime(2024, 4, 4), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(10, productId1, palletGuid9, 10, new DateTime(2024, 4, 4), DateOnly.FromDateTime(DateTime.Today.AddDays(366))),					
					ProductOnPallet.CreateForSeed(11, productId2, palletGuid9, 20, new DateTime(2024, 4, 4), DateOnly.FromDateTime(DateTime.Today.AddDays(366)))				
				);
			}

			if (!context.PalletMovements.Any())
			{
				context.PalletMovements.AddRange(
					new PalletMovement
					{
						Id = 1,
						PalletNumber = "Q1000",
						PalletId = palletGuid1,
						DestinationLocationId = 2,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 2,
						PalletNumber = "Q1001",
						PalletId = palletGuid2,
						DestinationLocationId = 1,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 3,
						PalletNumber = "Q1002",
						PalletId = palletGuid3,
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 4,
						PalletNumber = "Q1010",
						PalletId = palletGuid4,
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 5,
						PalletNumber = "Q1000",
						PalletId = palletGuid1,
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					}
				);
			}
			if (!context.PalletMovementDetails.Any())
			{
				context.PalletMovementDetails.AddRange(
					new PalletMovementDetail
					{
						Id = 1,
						PalletMovementId = 1,
						ProductId = productId1,
						Quantity = 100,
					},
					new PalletMovementDetail
					{
						Id = 6,
						PalletMovementId = 1,
						ProductId = productId2,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 2,
						PalletMovementId = 2,
						ProductId = productId2,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 3,
						PalletMovementId = 3,
						ProductId = productId2,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 4,
						PalletMovementId = 4,
						ProductId = productId1,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 5,
						PalletMovementId = 5,
						ProductId = productId1,
						Quantity = 1,
					});
			}
			if (!context.Inventories.Any())
			{

				context.Inventories.AddRange(
					new Inventory
					{
						ProductId = productId1,
						Quantity = 10,
						LastUpdated = new DateTime(2025, 5, 6)
					},
					new Inventory
					{
						ProductId = productId2,
						Quantity = 0,
						LastUpdated = new DateTime(2025, 5, 6)
					});
			}
			var reversePickingTaskId1 = Guid.Parse("11111111-1111-1111-2222-111111111111");
			var reversePickingTaskId2 = Guid.Parse("11111111-1111-1111-2333-111111111111");
			if (!context.ReversePickings.Any())
			{
				context.ReversePickings.AddRange(
					ReversePicking.CreateForSeed(reversePickingTaskId1, palletGuid9, null, productId1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),10,pickingId2,"UserR"),
					ReversePicking.CreateForSeed(reversePickingTaskId2, palletGuid9, null, productId1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)), 10, pickingId5, "UserR")
				
				);
			}
			context.SaveChanges();
		}
	}

}
