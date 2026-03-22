using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.Common
{
	public class DbContextFactory
	{
		public static Mock<WerehouseDbContext> Create()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var mock = new Mock<WerehouseDbContext>(options, null) { CallBase = true };
			var context = mock.Object;
			context.Database.EnsureCreated();
			SeedDatabase(context);
			return mock;
		}

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
					new IdentityUser { Id = "U001", UserName = "TestUser" },
					new IdentityUser { Id = "U002", UserName = "TestUser2" }
				);
			}

			if (!context.Categories.Any())
			{
				context.Categories.AddRange(
					new Category { Id = 1, Name = "TestCategory", IsDeleted = false },
					new Category { Id = 2, Name = "TestCategory1" },
					new Category { Id = 3, Name = "ToDeleted" }
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
				context.Products.AddRange(
					//Product.Create(productId1, "Test", "0987654321",  1, false, 56),
					//Product.Create(productId1, "TestD", "fghtredfg",  1, false, 44),
					//Product.Create(productId989, "NotAdded", "fghtredfg", 1, false, 112)
					new Product { Id = productId1, Name = "Test", SKU = "0987654321", CategoryId = 1, IsDeleted = false, CartonsPerPallet = 56, },
					new Product { Id = productId2, Name = "TestD", SKU = "fghtredfg", CategoryId = 1, IsDeleted = false, CartonsPerPallet = 44, },
					new Product { Id = productId989, Name = "NotAdded", SKU = "fghtredfg", CategoryId = 1, IsDeleted = false, CartonsPerPallet = 112, }
				);
			}

			if (!context.ProductDetails.Any())
			{
				context.ProductDetails.AddRange(
					new ProductDetail
					{
						ProductId = productId1,
						Length = 10,
						Height = 20,
						Width = 30,
						Weight = 2,
						Description = "TestDetails"
					},
					new ProductDetail
					{
						ProductId = productId2,
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
			var issueId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			if (!context.Issues.Any())
			{
				context.Issues.Add(new Issue
				{
					Id = issueId2,
					IssueNumber = 2,
					ClientId = 11,
					PerformedBy = "U002",
					IssueDateTimeCreate = new DateTime(2025, 5, 5),
					IssueDateTimeSend = new DateTime(2025, 5, 6),//zmiana 
					IssueItems = new List<IssueItem> { new IssueItem
					{
						ProductId = productId1,
						CreatedAt = DateTime.Today,
						BestBefore = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
						Quantity = 20,
					} }
				});
			}

			context.SaveChanges();
			// 3. Dane końcowe, zależne od receipt/issue/product

			var palletGuid1 = Guid.Parse("00000000-0001-1111-0000-000000000000");
			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");
			var palletGuid3 = Guid.Parse("00000000-0003-1111-0000-000000000000");
			var palletGuid4 = Guid.Parse("00000000-0004-1111-0000-000000000000");
			//var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");
			//var palletGuid6 = Guid.Parse("00000000-0006-1111-0000-000000000000");
			//var palletGuid7 = Guid.Parse("00000000-0007-1111-0000-000000000000");
			//var palletGuid8 = Guid.Parse("00000000-0008-1111-0000-000000000000");
			//var palletGuid9 = Guid.Parse("00000000-0009-1111-0000-000000000000");



			if (!context.Pallets.Any())
			{
				context.Pallets.AddRange(
					new Pallet
					{
						Id = palletGuid1,
						PalletNumber = "Q1000",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 1,
						Status = PalletStatus.Available,
						ReceiptId = receiptId1,
						IssueId = issueId2,
					},
					new Pallet
					{
						Id = palletGuid2,
						PalletNumber = "Q1001",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 1,
						Status = PalletStatus.OnHold,
						ReceiptId = receiptId1,
						IssueId = issueId2,
					},
					new Pallet
					{
						Id = palletGuid3,
						PalletNumber ="Q1002",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 3,
						Status = PalletStatus.Available,
						ReceiptId = receiptId2,
					},
					new Pallet
					{
						Id = palletGuid4,
						PalletNumber = "Q1010",
						DateReceived = new DateTime(2025, 1, 1),
						LocationId = 3,
						Status = PalletStatus.Damaged,
						ReceiptId = receiptId2,
					}
				);
			}

			if (!context.ProductOnPallet.Any())
			{
				context.ProductOnPallet.AddRange(
					new ProductOnPallet
					{
						Id = 1,
						ProductId = productId1,
						Quantity = 50,
						BestBefore = new DateOnly(2026, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						//PalletId = "Q1000"
						PalletId = palletGuid1
					},
					new ProductOnPallet
					{
						Id = 2,
						ProductId = productId1,
						Quantity = 100,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						//PalletId = "Q1001"
						PalletId = palletGuid2
					},
					new ProductOnPallet
					{
						Id = 3,
						ProductId = productId2,
						Quantity = 200,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						//PalletId = "Q1001"
						PalletId = palletGuid2
					},
					new ProductOnPallet
					{
						Id = 4,
						ProductId = productId2,
						Quantity = 200,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						//PalletId = "Q1010"
						PalletId = palletGuid3
					},
					new ProductOnPallet
					{
						Id = 5,
						ProductId = productId2,
						Quantity = 200,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						//PalletId = "Q1002"
						PalletId= palletGuid4
					}
				);
			}

			if (!context.PalletMovements.Any())
			{
				context.PalletMovements.AddRange(
					new PalletMovement
					{
						Id = 1,
						PalletId = palletGuid1,
						PalletNumber = "Q1000",
						//ProductId = 10,
						DestinationLocationId = 2,
						Reason = ReasonMovement.Moved,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 2,
						PalletId = palletGuid2,
						PalletNumber = "Q1001",
						//ProductId = 10,
						DestinationLocationId = 1,
						Reason = ReasonMovement.Moved,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 3,
						PalletId = palletGuid3,
						PalletNumber = "Q1002",
						//ProductId = 11,
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 4,
						PalletId = palletGuid4,
						PalletNumber = "Q1010",
						//ProductId = 10,
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2),
						PerformedBy = "TestUser",
					},
					new PalletMovement
					{
						Id = 5,
						PalletId = palletGuid1,
						PalletNumber = "Q1000",
						//ProductId = 10,
						DestinationLocationId = 3,
						Reason = ReasonMovement.Moved,
						//Quantity = 1,
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
						ProductId =productId2,
						Quantity = 1,
					},
					new PalletMovementDetail
					{
						Id = 3,
						PalletMovementId = 3,
						ProductId =productId2,
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
						//Id = 1,						
						ProductId = productId1,
						Quantity = 10,
						LastUpdated = new DateTime(2025, 5, 6)
					},
					new Inventory
					{
						//Id = 2,						
						ProductId = productId2,
						Quantity = 0,
						LastUpdated = new DateTime(2025, 5, 6)
					});
			}
			context.SaveChanges();
		}

		public static void Destroy(WerehouseDbContext context)
		{
			context.Database.EnsureDeleted();
			context.Dispose();
		}
	}
}
