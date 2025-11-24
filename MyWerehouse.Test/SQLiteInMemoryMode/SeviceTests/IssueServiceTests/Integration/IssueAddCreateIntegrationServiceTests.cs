using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;


namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueAddCreateIntegrationServiceTests : IssueIntegrationCommandService
	{
		//HappyPath
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_AssignsFullPalletsAndAllocatesRest_HappyPath()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
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
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 9, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				}
			};
			var issue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = DateTime.Now,
				IssueStatus = IssueStatus.New,
				IssueDateTimeSend = new DateTime(2025, 9, 9),
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem = new IssueItemDTO
			{
				ProductId = product.Id,
				Quantity = 26, // 2 pełne palety + 5 do pickingu
				BestBefore = new DateOnly(2025, 10, 10),
			};
			var result = await _issueService.AddPalletsToIssueByProductAsync(issue, issueItem);			

			// Assert
			Assert.True(result.Success);
			Assert.Contains("Towar dołączono do wydania", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			// Picking pallet
			var palletToPicking = DbContext.Pallets.FirstOrDefault(p => p.Id == "P2");
			var pickingPallet = DbContext.VirtualPallets.Include(pp => pp.Allocations).SingleOrDefault();
			Assert.NotNull(palletToPicking);
			Assert.NotNull(pickingPallet);
			Assert.Equal(6, pickingPallet.Allocations.First().Quantity);
			Assert.Equal(3, pickingPallet.RemainingQuantity);
			Assert.Equal("P2", pickingPallet.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking.Status);
			Assert.Equal(issue.Id, pickingPallet.Allocations.First().IssueId);
			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			Assert.Equal(3, movements.Count);
			Assert.Contains(movements, m => m.PalletId == "P1");
			Assert.Contains(movements, m => m.PalletId == "P2");
			Assert.Contains(movements, m => m.PalletId == "P3");
		}
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_AssignsFullAndRestPalletsAndAllocatesRestFromVirtualPallet_HappyPath()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
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
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.ToPicking,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 2, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				},
				new Pallet
				{
					Id = "P4",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 3, BestBefore = new DateOnly(2026,1,1) }
					}
				},
				new Pallet
				{
					Id = "P5",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 2, BestBefore = new DateOnly(2026,1,1) }
					}
				}
			};
			var issue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = DateTime.Now,
				IssueStatus = IssueStatus.New,
				IssueDateTimeSend = new DateTime(2025, 9, 9),
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};
			
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			var virtualPallet = new VirtualPallet
			{
				Allocations = [],
				IssueInitialQuantity = 2,
				PalletId = "P2",
				DateMoved = DateTime.UtcNow.AddDays(-7),
				Location = location2,
			};
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem = new IssueItemDTO
			{
				ProductId = product.Id,
				Quantity = 26, // 2 pełne palety + 5 do pickingu
				BestBefore = new DateOnly(2025, 10, 10),
			};
			var result = await _issueService.AddPalletsToIssueByProductAsync(issue, issueItem);

			// Assert
			Assert.True(result.Success);
			Assert.Contains("Towar dołączono do wydania", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			//Picking pallet
			var palletToPickingP2 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P2");
			var pickingPalletP2 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(x=>x.PalletId == "P2");
			Assert.NotNull(palletToPickingP2);
			Assert.NotNull(pickingPalletP2);
			Assert.Equal(2, pickingPalletP2.Allocations.First().Quantity);
			Assert.Equal(0, pickingPalletP2.RemainingQuantity);
			Assert.Equal("P2", pickingPalletP2.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPickingP2.Status);
			Assert.Equal(issue.Id, pickingPalletP2.Allocations.First().IssueId);

			var palletToPickingP4 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P4");
			var pickingPalletP4 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(x => x.PalletId == "P4");
			Assert.NotNull(palletToPickingP4);
			Assert.NotNull(pickingPalletP4);
			Assert.Equal(3, pickingPalletP4.Allocations.First().Quantity);
			Assert.Equal(0, pickingPalletP4.RemainingQuantity);
			Assert.Equal("P4", pickingPalletP4.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPickingP4.Status);
			Assert.Equal(issue.Id, pickingPalletP4.Allocations.First().IssueId);

			var palletToPickingP5 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P5");
			var pickingPalletP5 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(x => x.PalletId == "P5");
			Assert.NotNull(palletToPickingP5);
			Assert.NotNull(pickingPalletP5);
			Assert.Equal(1, pickingPalletP5.Allocations.First().Quantity);
			Assert.Equal(1, pickingPalletP5.RemainingQuantity);
			Assert.Equal("P5", pickingPalletP5.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPickingP5.Status);
			Assert.Equal(issue.Id, pickingPalletP5.Allocations.First().IssueId);
			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			Assert.Equal(4, movements.Count);
			Assert.Contains(movements, m => m.PalletId == "P1");
			Assert.Contains(movements, m => m.PalletId == "P5");
			Assert.Contains(movements, m => m.PalletId == "P3");			
			Assert.Contains(movements, m => m.PalletId == "P4");			
		}
		[Fact]
		public async Task CreateNewIssueAsync_AssignsFullPalletsAndAllocatesRest_HappyPath()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
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
			var location4 = new Location
			{
				Aisle = 4,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location5 = new Location
			{
				Aisle = 5,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory1 = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var initialCategory2 = new Category
			{
				Name = "name2",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory1,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			var product2 = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory2,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				},
				new Pallet
				{
					Id = "P4",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location4,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product2, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P5",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location5,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product2, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
			};

			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.AddRange(initialCategory1, initialCategory2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location1, location2, location3, location4, location5);
			DbContext.Pallets.AddRange(availablePallets);
			await DbContext.SaveChangesAsync();			
			var createIssue = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{   new IssueItemDTO
					{
						ProductId = product1.Id,
						Quantity = 26, // 2 pełne palety + 6 do pickingu
						BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
						//BestBefore = new DateOnly(2025, 10, 10)
					},  new IssueItemDTO
					{
						ProductId = product2.Id,
						Quantity = 17, // 1 pełne palety + 7 do pickingu
						BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
					}
				}
			};
			await _issueService.CreateNewIssueAsync(createIssue, DateTime.UtcNow.AddDays(7));

			// Assert
			var issue = DbContext.Issues.First();
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(3, issue.Pallets.Count); // 3 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			// Picking pallet
			var partialPallets = DbContext.Pallets.Where(p => p.Status == PalletStatus.ToPicking).ToList();
			Assert.Equal(2, partialPallets.Count); // P3, P5


			var palletToPicking1 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P3");
			var pickingPallet1 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(p => p.PalletId == "P3");
			Assert.NotNull(palletToPicking1);
			Assert.NotNull(pickingPallet1);
			Assert.Equal(6, pickingPallet1.Allocations.First().Quantity);
			//Assert.Equal(6, pickingPallet1.Allocation.FirstOrDefault(i=>i.IssueId == issue.Id).Quantity);
			Assert.Equal(4, pickingPallet1.RemainingQuantity);
			Assert.Equal("P3", pickingPallet1.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking1.Status);
			Assert.Equal(issue.Id, pickingPallet1.Allocations.First().IssueId);

			var palletToPicking2 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P5");
			var pickingPallet2 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(p => p.PalletId == "P5");
			Assert.NotNull(palletToPicking2);
			Assert.NotNull(pickingPallet2);
			Assert.Equal(7, pickingPallet2.Allocations.First().Quantity);
			Assert.Equal(3, pickingPallet2.RemainingQuantity);
			Assert.Equal("P5", pickingPallet2.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking2.Status);
			Assert.Equal(issue.Id, pickingPallet2.Allocations.First().IssueId);

			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			Assert.Equal(5, movements.Count);
			Assert.Contains(movements, m => m.PalletId == "P1");
			Assert.Contains(movements, m => m.PalletId == "P2");
			Assert.Contains(movements, m => m.PalletId == "P3");
			Assert.Contains(movements, m => m.PalletId == "P4");
			Assert.Contains(movements, m => m.PalletId == "P5");
		}

		[Fact]
		public async Task CreateNewIssueAsync_AssignsFullPalletsAndAllocatesRestWithPickingPalletIncludeOtherAllocation_HappyPath()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
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
			var location4 = new Location
			{
				Aisle = 4,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location5 = new Location
			{
				Aisle = 5,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory1 = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var initialCategory2 = new Category
			{
				Name = "name2",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory1,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			var product2 = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory2,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			//var availablePallets = new List<Pallet>
			var pallet1 = new Pallet
			{
				Id = "P1",
				DateReceived = new DateTime(2025, 3, 3),
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
			};
			var pallet2 = new Pallet
			{
				Id = "P2",
				DateReceived = new DateTime(2025, 3, 3),
				Location = location2,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
			};
			var pallet3 = new Pallet
			{
				Id = "P3",
				DateReceived = new DateTime(2025, 3, 3),
				Location = location3,
				Status = PalletStatus.ToPicking,//paleta jest już paletą źródłową pickingu
				ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
			};
			var pallet4 = new Pallet
			{
				Id = "P4",
				DateReceived = new DateTime(2025, 3, 3),
				Location = location4,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product2, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
			};
			var pallet5 = new Pallet
			{
				Id = "P5",
				DateReceived = new DateTime(2025, 3, 3),
				Location = location5,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product2, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
			};
			var oldIssue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = new DateTime(2025, 8, 8),
				//Pallets,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var sourcePallet = new VirtualPallet
			{
				Pallet = pallet3,
				Location = pallet3.Location,
				DateMoved = new DateTime(2025, 9, 1),
				IssueInitialQuantity = 10
			};
			var allocation = new Allocation
			{
				VirtualPallet = sourcePallet,
				Quantity = 2,
				PickingStatus = PickingStatus.Allocated,
				Issue = oldIssue
			};
			sourcePallet.Allocations = new List<Allocation> { allocation };
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.AddRange(initialCategory1, initialCategory2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location1, location2, location3, location4, location5);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4, pallet5);
			DbContext.Issues.Add(oldIssue);
			DbContext.Allocations.Add(allocation);
			DbContext.VirtualPallets.Add(sourcePallet);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem1 = new IssueItemDTO
			{
				ProductId = product1.Id,
				Quantity = 26, // 2 pełne palety + 6 do pickingu
				BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
			};
			var issueItem2 = new IssueItemDTO
			{
				ProductId = product2.Id,
				Quantity = 17, // 1 pełne palety + 7 do pickingu
				BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
			};
			var createIssue = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					issueItem1, issueItem2
				}
			};
			var result = await _issueService.CreateNewIssueAsync(createIssue, DateTime.UtcNow.AddDays(7));
			Assert.NotNull(result);
			// Assert
			Assert.NotNull(result);
			//Assert.False(result.S)
			var issue = DbContext.Issues.FirstOrDefault(i => i.Id == 2);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(3, issue.Pallets.Count); // 3 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			// Picking pallet
			var partialPallets = DbContext.Pallets.Where(p => p.Status == PalletStatus.ToPicking).ToList();
			Assert.Equal(2, partialPallets.Count); // P3, P5


			var palletToPicking1 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P3");
			var pickingPallet1 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(p => p.PalletId == "P3");
			Assert.NotNull(palletToPicking1);
			Assert.NotNull(pickingPallet1);
			//Assert.Equal(6, pickingPallet1.Allocation.First().Quantity);
			Assert.Equal(6, pickingPallet1.Allocations.FirstOrDefault(i => i.IssueId == issue.Id).Quantity);
			Assert.Equal(2, pickingPallet1.RemainingQuantity); //bo zarezerzowane z innego wydania
			Assert.Equal("P3", pickingPallet1.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking1.Status);
			//Assert.Equal(issue.Id, pickingPallet1.Allocation.FirstOrDefault(i=>i.).IssueId);

			var palletToPicking2 = DbContext.Pallets.FirstOrDefault(p => p.Id == "P5");
			var pickingPallet2 = DbContext.VirtualPallets.Include(pp => pp.Allocations).FirstOrDefault(p => p.PalletId == "P5");
			Assert.NotNull(palletToPicking2);
			Assert.NotNull(pickingPallet2);
			Assert.Equal(7, pickingPallet2.Allocations.First().Quantity);
			Assert.Equal(3, pickingPallet2.RemainingQuantity);
			Assert.Equal("P5", pickingPallet2.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking2.Status);
			Assert.Equal(issue.Id, pickingPallet2.Allocations.First().IssueId);

			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			//Assert.Equal(5, movements.Count);
			Assert.Contains(movements, m => m.PalletId == "P1");
			Assert.Contains(movements, m => m.PalletId == "P2");
			//Assert.Contains(movements, m => m.PalletId == "P3");
			Assert.Contains(movements, m => m.PalletId == "P4");
			Assert.Contains(movements, m => m.PalletId == "P5");
		}
		//SadPath
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_NotEnoughProduct_ThrowInfo()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
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
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				}
			};
			var issue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = DateTime.Now,
				IssueStatus = IssueStatus.Pending,
				IssueDateTimeSend = new DateTime(2025, 9, 9),
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem = new IssueItemDTO
			{
				ProductId = product.Id,
				Quantity = 31,
				BestBefore = new DateOnly(2025, 10, 10),
			};
			var result = await _issueService.AddPalletsToIssueByProductAsync(issue, issueItem);
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.False(result.Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}. Asortyment nie został dodany do zlecenia.", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(issueItem.Quantity, result.QuantityRequest);

			var stock = 30; //Quantity P1+P2+P3
			Assert.Equal(stock, result.QuantityOnStock);
		}

		[Fact]
		public async Task AddPalletsToIssueByProductAsync_BadBestBeforeProduct_ThrowInfo()
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
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
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2024,1,1) }
					}
				}
			};
			var issue = new Issue
			{
				Client = initailClient,
				IssueDateTimeCreate = DateTime.Now,
				IssueStatus = IssueStatus.New,
				IssueDateTimeSend = new DateTime(2025, 9, 9),
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem = new IssueItemDTO
			{
				ProductId = product.Id,
				Quantity = 25,
				BestBefore = new DateOnly(2025, 10, 10),
			};
			var result = await _issueService.AddPalletsToIssueByProductAsync(issue, issueItem);
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.False(result.Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}. Asortyment nie został dodany do zlecenia.", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(issueItem.Quantity, result.QuantityRequest);

			var stock = 20; //Quantity P1+P2 P3 BestBeforeWrong
			Assert.Equal(stock, result.QuantityOnStock);
		}

	}
}
