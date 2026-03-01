using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;

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

			if (!context.Products.Any())
			{
				context.Products.AddRange(
					new Product { Id = 10, Name = "Test", SKU = "0987654321", CategoryId = 1, IsDeleted = false, CartonsPerPallet = 56, },
					new Product { Id = 11, Name = "TestD", SKU = "fghtredfg", CategoryId = 1, IsDeleted = false, CartonsPerPallet = 44, },
					new Product { Id = 989, Name = "NotAdded", SKU = "fghtredfg", CategoryId = 1, IsDeleted = false, CartonsPerPallet = 112, }
				);
			}

			if (!context.ProductDetails.Any())
			{
				context.ProductDetails.AddRange(
					new ProductDetail
					{
						ProductId = 10,
						Length = 10,
						Height = 20,
						Width = 30,
						Weight = 2,
						Description = "TestDetails"
					},
					new ProductDetail
					{
						ProductId = 11,
						Length = 20,
						Height = 40,
						Width = 60,
						Weight = 3,
						Description = "TestDetails 11"
					}
					);
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
					new Receipt
					{
						Id = receiptId1,
						ReceiptNumber = 1,
						ClientId = 10,
						PerformedBy = "U001",
						ReceiptDateTime = new DateTime(2023, 3, 3),
					},
					new Receipt
					{
						Id = receiptId2,
						ReceiptNumber = 2,
						ClientId = 11,
						PerformedBy = "U002",
						ReceiptDateTime = new DateTime(2023, 4, 4)
					}
										);
			}
			var issuetId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			if (!context.Issues.Any())
			{
				context.Issues.Add(new Issue
				{
					Id = issuetId2,
					IssueNumber = 2,
					ClientId = 11,
					PerformedBy = "U002",
					IssueDateTimeCreate = DateTime.UtcNow.AddDays(-5),
					IssueDateTimeSend = DateTime.UtcNow.AddHours(23),//zmiana 
					IssueItems = new List<IssueItem> { new IssueItem
					{
						ProductId = 10,
						CreatedAt = DateTime.Today,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
						Quantity = 150,
					}, new IssueItem
					{
						ProductId = 11,
						CreatedAt = DateTime.Today,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
						Quantity = 400,
					}}
				});
			}

			context.SaveChanges();

			// 3. Dane końcowe, zależne od receipt/issue/product
			if (!context.Pallets.Any())
			{
				context.Pallets.AddRange(
					new Pallet
					{
						Id = "Q1000",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 1,
						Status = PalletStatus.Available,
						ReceiptId = receiptId1,
						IssueId = issuetId2,//
					},
					new Pallet
					{
						Id = "Q1001",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 1,
						Status = PalletStatus.OnHold,
						ReceiptId = receiptId1,
						IssueId = issuetId2,//
					},
					new Pallet
					{
						Id = "Q1002",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 3,
						Status = PalletStatus.Available,
						ReceiptId = receiptId2,
					},
					new Pallet
					{
						Id = "Q1010",
						DateReceived = new DateTime(2025, 1, 1),
						LocationId = 3,
						Status = PalletStatus.Damaged,
						ReceiptId = receiptId2,
					},
					new Pallet
					{
						Id = "Q1100",
						DateReceived = new DateTime(2025, 1, 1),
						LocationId = 3,
						Status = PalletStatus.ToPicking,
						ReceiptId = receiptId2,
					},
					new Pallet
					{
						Id = "Q1101",
						DateReceived = new DateTime(2025, 1, 5),
						LocationId = 3,
						Status = PalletStatus.ToPicking,
						ReceiptId = receiptId2,
					},
					new Pallet
					{
						Id = "Q2000",
						DateReceived = new DateTime(2025, 1, 1),
						LocationId = 3,
						Status = PalletStatus.ToIssue,
						ReceiptId =		receiptId2,
						IssueId = issuetId2,//
					},
					new Pallet
					{
						Id = "Q1200",
						DateReceived = new DateTime(2025, 2, 1),
						LocationId = 3,
						Status = PalletStatus.ToPicking,
						ReceiptId = receiptId2,
					},
					//PickingPallet
					new Pallet
					{
						Id = "Q5000",
						DateReceived = new DateTime(2025, 2, 1),
						LocationId = 3,
						Status = PalletStatus.ToPicking,
					}
				);
			}
			if (!context.VirtualPallets.Any())
			{
				context.VirtualPallets.AddRange(
					new VirtualPallet
					{
						Id = 1,
						PalletId = "Q1100",
						InitialPalletQuantity = 200,
						LocationId = 3,
						DateMoved = DateTime.UtcNow.AddDays(-1)						
					},
					new VirtualPallet
					{
						Id = 2,
						PalletId = "Q1101",
						InitialPalletQuantity = 150,
						LocationId = 3,
						DateMoved = new DateTime(2024, 6, 6),						
					},
					new VirtualPallet
					{
						Id = 3,
						PalletId = "Q1200",
						InitialPalletQuantity = 300,
						LocationId = 3,
						DateMoved = DateTime.UtcNow.AddDays(-1)						
					});
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
					new PickingTask
					{
						Id = pickingId1,
						//PickingTaskNumber = 1,
						VirtualPalletId = 1,
						PickingStatus = PickingStatus.Allocated,
						IssueId = issuetId2,
						RequestedQuantity = 20,
						ProductId = 11,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2))
						
					}, new PickingTask
					{
						Id = pickingId2,
						//PickingTaskNumber = 2,
						VirtualPalletId = 1,
						PickingStatus = PickingStatus.Picked,
						IssueId = issuetId2,
						RequestedQuantity = 20,
						ProductId = 11,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2))
					}, new PickingTask
					{
						Id = pickingId3,
						//PickingTaskNumber = 3,
						VirtualPalletId = 2,
						PickingStatus = PickingStatus.Allocated,
						IssueId = issuetId2,
						RequestedQuantity = 50,
						ProductId = 11,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2))
					}, new PickingTask
					{
						Id = pickingId4,
						//PickingTaskNumber = 4,
						VirtualPalletId = 3,
						PickingStatus = PickingStatus.Allocated,
						IssueId = issuetId2,
						RequestedQuantity = 100,
						ProductId = 10,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2))
					}, new PickingTask
					{
						Id = pickingId5,
						//PickingTaskNumber = 5,
						VirtualPalletId = 3,
						PickingStatus = PickingStatus.Picked,
						IssueId = issuetId2,
						RequestedQuantity = 10,
						ProductId = 10,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2))
					},
					new PickingTask
					{
						Id = pickingId6,
						//PickingTaskNumber = 6,
						VirtualPalletId = 1,
						PickingStatus = PickingStatus.Allocated,
						IssueId = issuetId2,
						RequestedQuantity = 20,
						ProductId = 11,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(23).AddDays(-2))
					}
					);
			}
			if (!context.ProductOnPallet.Any())
			{
				context.ProductOnPallet.AddRange(
					new ProductOnPallet
					{
						Id = 1,
						ProductId = 10,
						Quantity = 50,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1000"
					},
					new ProductOnPallet
					{
						Id = 2,
						ProductId = 10,
						Quantity = 100,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1001"
					},
					new ProductOnPallet
					{
						Id = 3,
						ProductId = 11,
						Quantity = 200,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1000"
					},
					new ProductOnPallet
					{
						Id = 4,
						ProductId = 11,
						Quantity = 200,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1010"
					},
					new ProductOnPallet
					{
						Id = 5,
						ProductId = 11,
						Quantity = 200,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1002"
					},
					new ProductOnPallet
					{
						Id = 6,
						ProductId = 11,
						Quantity = 200,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1100"
					},
					new ProductOnPallet//Issue
					{
						Id = 7,
						ProductId = 11,
						Quantity = 200,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q2000"
					},
					new ProductOnPallet
					{
						Id = 8,
						ProductId = 11,
						Quantity = 150,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 3, 3),
						PalletId = "Q1101"
					},
					new ProductOnPallet
					{
						Id = 9,
						ProductId = 10, // inny produkt
						Quantity = 300,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 4, 4),
						PalletId = "Q1200"
					},
					new ProductOnPallet
					{
						Id = 10,
						ProductId = 10,
						Quantity = 10,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 4, 4),
						PalletId = "Q5000"
					},
					new ProductOnPallet
					{
						Id = 11,
						ProductId = 11,
						Quantity = 20,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddDays(366)),
						DateAdded = new DateTime(2024, 4, 4),
						PalletId = "Q5000"
					}
				);
			}

			if (!context.PalletMovements.Any())
			{
				context.PalletMovements.AddRange(
					new PalletMovement
					{
						Id = 1,
						PalletId = "Q1000",						
						DestinationLocationId = 2,
						Reason = ReasonMovement.Moved,						
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 2,
						PalletId = "Q1001",						
						DestinationLocationId = 1,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 3,
						PalletId = "Q1002",
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 4,
						PalletId = "Q1010",
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 5,
						PalletId = "Q1000",
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
						ProductId = 10,
						Quantity = 100,
					},
					new PalletMovementDetail
					{
						Id = 6,
						PalletMovementId = 1,
						ProductId = 11,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 2,
						PalletMovementId = 2,
						ProductId = 11,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 3,
						PalletMovementId = 3,
						ProductId = 11,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 4,
						PalletMovementId = 4,
						ProductId = 10,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 5,
						PalletMovementId = 5,
						ProductId = 10,
						Quantity = 1,
					});
			}
			if (!context.Inventories.Any())
			{

				context.Inventories.AddRange(
					new Inventory
					{											
						ProductId = 10,
						Quantity = 10,
						LastUpdated = new DateTime(2025, 5, 6)
					},
					new Inventory
					{										
						ProductId = 11,
						Quantity = 0,
						LastUpdated = new DateTime(2025, 5, 6)
					});
			}
			var reversePickingTaskId1 = Guid.Parse("11111111-1111-1111-2222-111111111111");
			var reversePickingTaskId2 = Guid.Parse("11111111-1111-1111-2333-111111111111");
			if (!context.ReversePickings.Any())
			{
				context.ReversePickings.AddRange(
					new ReversePicking
					{
						Id = reversePickingTaskId1,
						PickingTaskId = pickingId2,
						ProductId = 10,
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
						PickingPalletId = "Q5000",
						Quantity = 10,
						Status = ReversePickingStatus.Pending,
						UserId = "UserR"

					},
					new ReversePicking
					{
						Id = reversePickingTaskId2,
						PickingTaskId = pickingId5,
						ProductId = 10,
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
						PickingPalletId = "Q5000",
						Quantity = 10,
						Status = ReversePickingStatus.Pending,
						UserId = "UserR"
					}
				);
			}
			context.SaveChanges();
		}
	}

}
