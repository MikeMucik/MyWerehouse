using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.Common
{
	public class DbContextFactory
	{
		public static Mock<WerehouseDbContext> Create()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var mock = new Mock<WerehouseDbContext>(options) { CallBase = true };
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
					new Category { Id = 3, Name = "ToDeleted"}
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

			if (!context.Receipts.Any())
			{
				context.Receipts.AddRange(
					new Receipt
					{
						Id = 1,
						ClientId = 10,
						PerformedBy = "U001",
						ReceiptDateTime = new DateTime(2023, 3, 3)
					},
					new Receipt
					{
						Id = 2,
						ClientId = 11,
						PerformedBy = "U002",
						ReceiptDateTime = new DateTime(2023, 4, 4)
					}
					);
			}
			if (!context.Issues.Any())
			{
				context.Issues.Add(new Issue
				{
					Id = 2,
					ClientId = 11,
					PerformedBy = "U002",
					IssueDateTimeCreate = new DateTime(2025, 5, 5),
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
						ReceiptId = 1,
						IssueId = 2,
					},
					new Pallet
					{
						Id = "Q1001",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 1,
						Status = PalletStatus.OnHold,
						ReceiptId = 1,
						IssueId = 2,
					},
					new Pallet
					{
						Id = "Q1002",
						DateReceived = new DateTime(2020, 1, 1),
						LocationId = 3,
						Status = PalletStatus.Available,
						ReceiptId = 2,
					},
					new Pallet
					{
						Id = "Q1010",
						DateReceived = new DateTime(2025, 1, 1),
						LocationId = 3,
						Status = PalletStatus.Damaged,
						ReceiptId = 2,
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
						BestBefore = new DateOnly(2026, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1000"
					},
					new ProductOnPallet
					{
						Id = 2,
						ProductId = 10,
						Quantity = 100,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1001"
					},
					new ProductOnPallet
					{
						Id = 3,
						ProductId = 11,
						Quantity = 200,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1001"
					},
					new ProductOnPallet
					{
						Id = 4,
						ProductId = 11,
						Quantity = 200,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1010"
					},
					new ProductOnPallet
					{
						Id = 5,
						ProductId = 11,
						Quantity = 200,
						BestBefore = new DateOnly(2025, 2, 2),
						DateAdded = new DateTime(2024, 2, 2),
						PalletId = "Q1002"
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
						//ProductId = 10,
						DestinationLocationId = 2,
						Reason = ReasonMovement.ManualMove,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2)
					},
					new PalletMovement
					{
						Id = 2,
						PalletId = "Q1001",
						//ProductId = 10,
						DestinationLocationId = 1,
						Reason = ReasonMovement.ManualMove,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2)
					},
					new PalletMovement
					{
						Id = 3,
						PalletId = "Q1002",
						//ProductId = 11,
						DestinationLocationId = 3,
						Reason = ReasonMovement.ManualMove,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2)
					},
					new PalletMovement
					{
						Id = 4,
						PalletId = "Q1010",
						//ProductId = 10,
						DestinationLocationId = 3,
						Reason = ReasonMovement.ManualMove,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2)
					},
					new PalletMovement
					{
						Id = 5,
						PalletId = "Q1000",
						//ProductId = 10,
						DestinationLocationId = 3,
						Reason = ReasonMovement.ManualMove,
						//Quantity = 1,
						MovementDate = new DateTime(2025, 2, 2)
					}
				);
			}
			if(!context.PalletMovementDetails.Any())
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
						//Id = 1,						
						ProductId = 10,
						Quantity = 10,
						LastUpdated = new DateTime(2025, 5, 6)
					},
					new Inventory
					{
						//Id = 2,						
						ProductId = 11,
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
