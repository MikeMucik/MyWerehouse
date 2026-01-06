using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Unit
{
	public class PickingServiceUnitTests : TestBase
	{
		[Fact]
		public async Task GetListToPicking_ShouldGroupByClientIssueAndProduct()
		{
			// Arrange		
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),				
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 50
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "667",
				AddedItemAd = new DateTime(2025, 2, 2),
				//CategoryId = category.Id,
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
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
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 2
			};
			var location4 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 3
			};
			var address1 = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address2 = new Address
			{
				City = "Cracow",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "mar",
				Phone = 222222,
				Region = "Małopolskie",
				StreetNumber = "45/3"
			};
			var client1 = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address1],
				IsDeleted = false,
			};
			var client2 = new Client
			{
				Name = "Client B",
				Email = "333@wp.pl",
				Description = "des2",
				FullName = "full333",
				Addresses = [address2],
				IsDeleted = false,
			};			
			var pallet1 = new Pallet
			{
				Id = "Q10",
				DateReceived = new DateTime(2025, 8, 8),				
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product = product1,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var pallet2 = new Pallet
			{
				Id = "Q11",
				DateReceived = new DateTime(2025, 9, 9),				
				Location = location2,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{	
						Product = product1,
						Quantity = 50,
						DateAdded = new DateTime(2025, 9, 9)
					}
				}
			};
			var pallet3 = new Pallet
			{
				Id = "Q12",
				DateReceived = new DateTime(2025, 10, 10),
				Location = location3,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						//ProductId = product2.Id,
						Product = product2,
						Quantity = 70,
						DateAdded = new DateTime(2025, 10, 10)
					}
				}
			};
			var pallet4 = new Pallet
			{
				Id = "Q13",
				DateReceived = new DateTime(2025, 11, 11),
				Location = location4,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						//ProductId = product2.Id,
						Product= product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 11, 11) }
				}
			};
			var issue1 = new Issue
			{				
				Client = client1,
				IssueDateTimeCreate = new DateTime(2025, 8, 12),				
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue2 = new Issue
			{
				Client = client1,
				IssueDateTimeCreate = new DateTime(2025, 8, 12),				
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue3 = new Issue
			{
				Client = client2,
				IssueDateTimeCreate = new DateTime(2025, 8, 12),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};

			DbContext.Addresses.AddRange(address1, address2);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, location3, location4);
			DbContext.Clients.AddRange(client1, client2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Issues.AddRange(issue1, issue2, issue3);			
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = pallet1,
				IssueInitialQuantity = 40,
				Location = pallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a11 = new Allocation { Issue = issue1, Quantity = 10, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet1 };
			var a12 = new Allocation { Issue = issue2, Quantity = 15, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet1 };
			virtualPallet1.Allocations = new List<Allocation> { a11, a12 };

			var virtualPallet2 = new VirtualPallet
			{
				Pallet = pallet2,
				IssueInitialQuantity = 50,
				Location = pallet2.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a21 = new Allocation { Issue = issue1, Quantity = 20, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet2 };
			var a22 = new Allocation { Issue = issue3, Quantity = 25, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet2 };
			virtualPallet2.Allocations = new List<Allocation> { a21, a22 };

			var virtualPallet3 = new VirtualPallet
			{
				Pallet = pallet3,
				IssueInitialQuantity = 50,
				Location = pallet3.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a31 = new Allocation { Issue = issue2, Quantity = 15, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet3 };
			virtualPallet3.Allocations = new List<Allocation> { a31 };

			var virtualPallet4 = new VirtualPallet
			{
				Pallet = pallet4,
				IssueInitialQuantity = 40,
				Location = pallet4.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a41 = new Allocation { Issue = issue1, Quantity = 10, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet4 };
			virtualPallet4.Allocations = new List<Allocation> { a41 };

			DbContext.Allocations.AddRange(a11, a12, a21, a22, a31, a41);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3, virtualPallet4);
			await DbContext.SaveChangesAsync();

			//var pickingPalletRepo = new PickingPalletRepo(DbContext);	
			//var allocationRepo = new AllocationRepo(DbContext);
			//var issueRepo = new IssueRepo(DbContext);
			//var locationRepo = new Mock<ILocationRepo>();
			//var palletRepo = new Mock<IPalletRepo>();
			//var palletService = new Mock<IPalletService>();
			//var eventCollector = new Mock<IEventCollector>();
			var service = new PickingPalletService(Mediator
				//, pickingPalletRepo,
				//	allocationRepo,
				//DbContext,
				//locationRepo.Object,
				//palletRepo.Object,
				//issueRepo, 
				//palletService.Object
				//,eventCollector.Object
				);

			// Act
			var result = await service.GetListToPickingAsync(
				new DateTime(2024, 8, 12),
				new DateTime(2026, 8, 12));

			// Assert
			result.Should().HaveCount(5); //Tu daje 5 bo daje alokacje każdą osobno

			// Client1, Issue1, Product1 → 10 + 20 = 30
			result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue1.Id,
				ProductId = product1.Id,
				Quantity = 30
			});

			// Client1, Issue2, Product2 → 15
			result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue2.Id,
				ProductId = product2.Id,
				Quantity = 15
			});

			// Client2, Issue3, Product1 → 25
			result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client2.Id,
				IssueId = issue3.Id,
				ProductId = product1.Id,
				Quantity = 25
			});
		}

		//Nowa metoda inne dane wyjściowe
		[Fact]
		public async Task GetListIssueToPickingAsync_ShouldGroupByClientIssueAndProduct()
		{
			// Arrange		
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),				
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 50
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "667",
				AddedItemAd = new DateTime(2025, 2, 2),				
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
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
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 2
			};
			var location4 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 3
			};
			var address1 = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address2 = new Address
			{
				City = "Cracow",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "mar",
				Phone = 222222,
				Region = "Małopolskie",
				StreetNumber = "45/3"
			};
			var client1 = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address1],
				IsDeleted = false,
			};
			var client2 = new Client
			{
				Name = "Client B",
				Email = "333@wp.pl",
				Description = "des2",
				FullName = "full333",
				Addresses = [address2],
				IsDeleted = false,
			};

			var pallet1 = new Pallet
			{
				Id = "Q10",
				DateReceived = new DateTime(2025, 8, 8),				
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product = product1,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var pallet2 = new Pallet
			{
				Id = "Q11",
				DateReceived = new DateTime(2025, 9, 9),				
				Location = location2,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{							
						Product = product1,
						Quantity = 50,
						DateAdded = new DateTime(2025, 9, 9)
					}
				}
			};
			var pallet3 = new Pallet
			{
				Id = "Q12",
				DateReceived = new DateTime(2025, 10, 10),
				Location = location3,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product = product2,
						Quantity = 70,
						DateAdded = new DateTime(2025, 10, 10)
					}
				}
			};
			var pallet4 = new Pallet
			{
				Id = "Q13",
				DateReceived = new DateTime(2025, 11, 11),
				Location = location4,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product= product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 11, 11) }
				}
			};
			var issue1 = new Issue
			{				
				Client = client1,
				IssueDateTimeCreate = new DateTime(2025, 8, 12),
				//Pallets,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue2 = new Issue
			{				
				Client = client1,
				IssueDateTimeCreate = new DateTime(2025, 8, 12),
				//Pallets,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue3 = new Issue
			{				
				Client = client2,
				IssueDateTimeCreate = new DateTime(2025, 8, 12),
				//Pallets,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};

			DbContext.Addresses.AddRange(address1, address2);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, location3, location4);
			DbContext.Clients.AddRange(client1, client2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Issues.AddRange(issue1, issue2, issue3);			

			var virtualPallet1 = new VirtualPallet
			{
				Pallet = pallet1,
				IssueInitialQuantity = 40,
				Location = pallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a11 = new Allocation { Issue = issue1, Quantity = 10, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet1 };
			var a12 = new Allocation { Issue = issue2, Quantity = 15, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet1 };
			virtualPallet1.Allocations = new List<Allocation> { a11, a12 };

			var virtualPallet2 = new VirtualPallet
			{
				Pallet = pallet2,
				IssueInitialQuantity = 50,
				Location = pallet2.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a21 = new Allocation { Issue = issue1, Quantity = 20, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet2 };
			var a22 = new Allocation { Issue = issue3, Quantity = 25, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet2 };
			virtualPallet2.Allocations = new List<Allocation> { a21, a22 };

			var virtualPallet3 = new VirtualPallet
			{
				Pallet = pallet3,
				IssueInitialQuantity = 50,
				Location = pallet3.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a31 = new Allocation { Issue = issue3, Quantity = 15, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet3 };
			virtualPallet3.Allocations = new List<Allocation> { a31 };

			var virtualPallet4 = new VirtualPallet
			{
				Pallet = pallet4,
				IssueInitialQuantity = 40,
				Location = pallet4.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a41 = new Allocation { Issue = issue1, Quantity = 10, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet4 };
			virtualPallet4.Allocations = new List<Allocation> { a41 };

			DbContext.Allocations.AddRange(a11, a12, a21, a22, a31, a41);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3, virtualPallet4);
			await DbContext.SaveChangesAsync();
			//var pickingPalletRepo = new PickingPalletRepo(DbContext);
			//var allocationRepo = new AllocationRepo(DbContext);
			//var issueRepo = new IssueRepo(DbContext);
			//var mapper = new Mock<IMapper>();
			//var locationRepo = new Mock<ILocationRepo>();
			//var palletRepo = new Mock<IPalletRepo>();		
			//var palletService = new Mock<IPalletService>();
			//var eventCollector = new Mock<IEventCollector>();
			var service = new PickingPalletService(Mediator
				//, pickingPalletRepo,
				//allocationRepo,
				//DbContext,
				//locationRepo.Object,
				//palletRepo.Object,
				//issueRepo,
				//palletService.Object
				//,eventCollector.Object
				);

			// Act
			var result = await service.GetListIssueToPickingAsync(
				new DateTime(2024, 8, 12),
				new DateTime(2026, 8, 12));

			// Assert
			result.Should().HaveCount(2);// Tu mi liczy klientów - dlatego 2
					
			// 1. Sprawdź, że w ogóle mamy jakiegoś klienta
			Assert.NotNull(result);
			Assert.NotEmpty(result);

			// 2. Pierwszy klient istnieje
			var firstClient = result.First();
			Assert.Equal(client1.Id, firstClient.ClientIdOut);

			// 3. Pierwszy klient ma dokładnie 2 zlecenia
			Assert.Equal(2, firstClient.Issues.Count);

			// 4. Pierwsze zlecenie klienta ma 2 produkty
			var firstIssue = firstClient.Issues.First();
			Assert.Equal(client1.Id, firstIssue.IssueId);
			Assert.Equal(2, firstIssue.Products.Count);

			// 5. Drugie zlecenie klienta ma 1 produkt
			var secondIssue = firstClient.Issues.Skip(1).First();
			Assert.Equal(client2.Id, secondIssue.IssueId);
			Assert.Single(secondIssue.Products);

			// --- Klient 1 ---
			var client1Result = result.Should().ContainSingle(r => r.ClientIdOut == client1.Id).Subject;

			// Klient1 ma 2 zlecenia
			client1Result.Issues.Should().HaveCount(2);

			// Klient1, Issue1, Product1 → 10 + 20 = 30
			var issue1Result = client1Result.Issues.Should().ContainSingle(i => i.IssueId == issue1.Id).Subject;
			issue1Result.Products.Should().ContainSingle(p => p.ProductId == product1.Id && p.Quantity == 30);

			// Klient1, Issue2, Product2 → 15
			var issue2Result = client1Result.Issues.Should().ContainSingle(i => i.IssueId == issue2.Id).Subject;
			issue2Result.Products.Should().ContainSingle(p => p.ProductId == product1.Id && p.Quantity == 15);

			// --- Klient 2 ---
			var client2Result = result.Should().ContainSingle(r => r.ClientIdOut == client2.Id).Subject;

			// Klient2 ma 1 zlecenie
			client2Result.Issues.Should().HaveCount(1);

			// Klient2, Issue3, Product1 → 25
			var issue3Result = client2Result.Issues.Should().ContainSingle(i => i.IssueId == issue3.Id).Subject;
			issue3Result.Products.Should().ContainSingle(p => p.ProductId == product1.Id && p.Quantity == 25);

		}
	}
}
