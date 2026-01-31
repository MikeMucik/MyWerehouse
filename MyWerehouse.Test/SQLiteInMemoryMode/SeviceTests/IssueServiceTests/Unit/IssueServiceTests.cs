using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Validators;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Unit
{
	public class IssueServiceTests : TestBase
	{
		[Fact]
		public async Task FinishIssueNotCompleted_ShouldUpdateStatusesAndCreateMovementsAndHistory()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailLocation = new Location
			{
				Id = 10,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initailLocation1 = new Location
			{
				Id = 20,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Id = 101,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var issueId = 1;
			var performedBy = "Janek";
			var loadedPallet = new Pallet
			{
				Id = "P1",
				Status = PalletStatus.Loaded,
				LocationId = 10,
				ProductsOnPallet = new List<ProductOnPallet>
		{
			new ProductOnPallet { ProductId = 101, Quantity = 5, }
		}
			};
			var notLoadedPallet = new Pallet
			{
				Id = "P2",
				Status = PalletStatus.OnHold,
				LocationId = 20,
				ProductsOnPallet = new List<ProductOnPallet>()
			};
			var issue = new Issue
			{
				Id = issueId,
				ClientId = initailCLient.Id,
				IssueDateTimeCreate = new DateTime(2025, 6, 6, 2, 2, 2),
				Pallets = new List<Pallet> { loadedPallet, notLoadedPallet },
				PerformedBy = "TestUser",
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var inventory = new Inventory
			{
				Product = initialProduct,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			var inventory1 = new Inventory
			{
				Product = initialProduct1,
				LastUpdated = DateTime.Now.AddDays(-7),
				Quantity = 100
			};
			DbContext.Inventories.AddRange(inventory, inventory1);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.AddRange(initailLocation, initailLocation1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();		

			var service = new IssueService(
				Mediator);

			// Act
			await service.FinishIssueNotCompleted(issueId, performedBy);

			// Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(PalletStatus.Available, notLoadedPallet.Status);
			Assert.Null(notLoadedPallet.IssueId);

			var updatedIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstOrDefaultAsync(i => i.Id == issueId);

			Assert.NotNull(updatedIssue);
			Assert.Equal(IssueStatus.IsShipped, updatedIssue.IssueStatus);

			// sprawdź czy P2 została usunięta z przypisania do zlecenia:
			var palletP2 = await DbContext.Pallets.FindAsync("P2");
			Assert.Equal(PalletStatus.Available, palletP2.Status);
			Assert.Null(palletP2.IssueId);
		}
		//[Fact]
		//public async Task AddPalletsToIssueByProductAsync_ShouldAssignFullPallets_WhenSufficientStock()
		//{
		//	// Arrange
		//	var issue = new Issue { Id = 1, PerformedBy = "user" };
		//	var product = new IssueItemDTO { ProductId = 10, Quantity = 200, BestBefore = new DateOnly(2025, 8, 1) };
		//	var cartonsPerPallet = 100;
		//	var pallets = new List<Pallet>
		//		{
		//			new() { Id = "Q1000", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = cartonsPerPallet, BestBefore = new DateOnly(2026, 8, 1) } }, LocationId = 1 },
		//			new() { Id = "Q1001", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = cartonsPerPallet, BestBefore = new DateOnly(2026, 8, 1) } }, LocationId = 2 }
		//		};
		//	//DbContext.Pallets.AddRange(pallets);
		//	//DbContext.SaveChanges();
		//	var mockMapper = new Mock<IMapper>();
		//	var issueRepoMock = new Mock<IIssueRepo>();
		//	issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
		//				 .ReturnsAsync(issue);

		//	var historyServiceMock = new Mock<IHistoryService>();
		//	var inventoryRepoMock = new Mock<IInventoryRepo>();
		//	var productRepoMock = new Mock<IProductRepo>();
		//	var palletRepoMock = new Mock<IPalletRepo>();
		//	var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//	var palletService = new Mock<IPalletService>();
		//	palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
		//		.Returns(pallets.AsQueryable());

		//	productRepoMock.Setup(r => r.GetProductByIdAsync(10))
		//		.ReturnsAsync(new Product { Id = 10, CartonsPerPallet = cartonsPerPallet });

		//	inventoryRepoMock.Setup(r => r.GetAvailableQuantityAsync(10, product.BestBefore))
		//		.ReturnsAsync(cartonsPerPallet * pallets.Count);

		//	var validator = new Mock<IValidator<CreateIssueDTO>>();

		//	var service = new IssueService(
		//		issueRepoMock.Object,
		//		mockMapper.Object,
		//		DbContext,
		//		historyServiceMock.Object,
		//		inventoryRepoMock.Object,
		//		palletRepoMock.Object,
		//		productRepoMock.Object,
		//		pickingRepoMock.Object,
		//		palletService.Object,
		//		validator.Object
		//	);
		//	// Act
		//	await service.AddPalletsToIssueByProductAsync(issue, product);

		//	// Assert
		//	Assert.Equal(2, issue.Pallets.Count);
		//	Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
		//}
		//[Fact]
		//public async Task AddPalletsToIssueByProductAsync_ShouldThrow_WhenInsufficientStock() //Zmień na synchroniczny bo mock
		//{
		//	// Arrange
		//	var issue = new Issue { Id = 1 };
		//	var product = new IssueItemDTO { ProductId = 10, Quantity = 300, BestBefore = new DateOnly(2025, 8, 1) };
		//	var pallets = new List<Pallet>
		//		{
		//			new() { Id = "Q1000", ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } } },
		//			new() { Id = "Q1001", ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } } }
		//		};
		//	var mockMapper = new Mock<IMapper>();
		//	var issueRepoMock = new Mock<IIssueRepo>();
		//	issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
		//				 .ReturnsAsync(issue);

		//	var historyServiceMock = new Mock<IHistoryService>();
		//	var inventoryRepoMock = new Mock<IInventoryRepo>();
		//	var productRepoMock = new Mock<IProductRepo>();
		//	var palletRepoMock = new Mock<IPalletRepo>();
		//	var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//	var palletService = new Mock<IPalletService>();
		//	palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
		//		.Returns(pallets.AsQueryable());

		//	var validator = new Mock<IValidator<CreateIssueDTO>>();

		//	var service = new IssueService(
		//		issueRepoMock.Object,
		//		mockMapper.Object,
		//		DbContext,
		//		historyServiceMock.Object,
		//		inventoryRepoMock.Object,
		//		palletRepoMock.Object,
		//		productRepoMock.Object,
		//		pickingRepoMock.Object,
		//		palletService.Object,
		//		validator.Object
		//	);
		//	//Act
		//	var result = await service.AddPalletsToIssueByProductAsync(issue, product);
		//	//Assert
		//	Assert.False(result.Success);
		//	Assert.Contains("Brak odpowiedniej ilości towaru", result.Message);
		//	//Assert.Equal(10, result.ProductId);
		//	//Assert.Equal(300, result.QuantityRequest);
		//	//Assert.Equal(200, result.QuantityOnStock);
		//}
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_AssignsFullPalletsAndAllocatesRest()
		{
			// Arrange
			var productId = 100;
			var bestBefore = new DateOnly(2025, 10, 10);
			var cartonsPerPallet = 10;
			var issueItem = new IssueItemDTO
			{
				ProductId = productId,
				Quantity = 25, // 2 pełne palety + 5 do pickingu
				BestBefore = bestBefore,
			};
			var availablePallets = new List<Pallet>
				{
				new Pallet
					{
						Id = "P1",
					LocationId = 1,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10,
					BestBefore =new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					LocationId = 2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10,
							BestBefore =new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					LocationId = 1,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore =new DateOnly(2026,1,1) }
					}
				}
			};
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var issue = new Issue
			{
				//Id = 1,
				ClientId = initailCLient.Id,
				IssueDateTimeCreate = DateTime.Now,
				IssueDateTimeSend = DateTime.Now.AddDays(7),
				IssueStatus = IssueStatus.Pending,
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};
			DbContext.Clients.Add(initailCLient);
			DbContext.Locations.AddRange(new Location { Id = 1, Aisle = 1, Bay = 1, Height = 1, Position = 1 }, new Location { Id = 2, Aisle = 2, Bay = 1, Height = 1, Position = 1 });
			DbContext.Categories.Add(new Category { Id = 1, Name = "tested" });
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Products.Add(new Product { Id = productId, CartonsPerPallet = cartonsPerPallet, Name = "test", SKU = "123", CategoryId = 1 });
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			var service = new IssueService(Mediator);
			// Act
			await service.AddPalletsToIssueByProductAsync(issue, issueItem);
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count);
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));						
		}
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_AssignsFullPalletsAndAllocatesRest_FullIntegration()
		{
			// Arrange
			var productId = 100;
			var bestBefore = new DateOnly(2025, 10, 10);
			var cartonsPerPallet = 10;

			var issueItem = new IssueItemDTO
			{
				ProductId = productId,
				Quantity = 25, // 2 pełne palety + 5 do pickingu
				BestBefore = bestBefore,
			};

			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					LocationId = 1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					LocationId = 2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					LocationId = 1,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
					}
				}
			};
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
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var issue = new Issue
			{
				ClientId = initailClient.Id,
				IssueDateTimeCreate = DateTime.Now,
				IssueDateTimeSend = DateTime.Now.AddDays(7),
				IssueStatus = IssueStatus.New,
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};

			// Dodanie do DbContext
			DbContext.Clients.Add(initailClient);
			DbContext.Locations.AddRange(
				new Location { Id = 1, Aisle = 1, Bay = 1, Height = 1, Position = 1 },
				new Location { Id = 2, Aisle = 2, Bay = 1, Height = 1, Position = 1 }
			);
			DbContext.Categories.Add(new Category { Id = 1, Name = "tested" });
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Products.Add(new Product { Id = productId, CartonsPerPallet = cartonsPerPallet, Name = "test", SKU = "123", CategoryId = 1 });
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			var service = new IssueService(Mediator);

			// Act
			await service.AddPalletsToIssueByProductAsync(issue, issueItem);
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
		}
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_NotEnoughProduct_NotFullIntegration()
		{
			// Arrange
			var productId = 100;
			var bestBefore = new DateOnly(2025, 10, 10);
			var cartonsPerPallet = 10;
			var issueItem = new IssueItemDTO
			{
				ProductId = productId,
				Quantity = 100, // brakuje 70
				BestBefore = bestBefore,
			};
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					LocationId = 1,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P2",
					LocationId = 2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					LocationId = 1,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { ProductId = productId, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				}
			};
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
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var issue = new Issue
			{
				ClientId = initailClient.Id,
				IssueDateTimeCreate = DateTime.Now,
				IssueStatus = IssueStatus.New,
				Pallets = new List<Pallet>(),
				PerformedBy = "TestUser",
			};
			// Dodanie do DbContext
			DbContext.Clients.Add(initailClient);
			DbContext.Locations.AddRange(
				new Location { Id = 1, Aisle = 1, Bay = 1, Height = 1, Position = 1 },
				new Location { Id = 2, Aisle = 2, Bay = 1, Height = 1, Position = 1 }
			);
			DbContext.Categories.Add(new Category { Id = 1, Name = "tested" });
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Products.Add(new Product { Id = productId, CartonsPerPallet = cartonsPerPallet, Name = "test", SKU = "123", CategoryId = 1 });
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			
			//var pickingPalletRepo = new Mock<IPickingPalletRepo>();

			var service = new IssueService(Mediator);
			//Act
			var result = await service.AddPalletsToIssueByProductAsync(issue, issueItem);
			//Assert
			Assert.False(result.Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {productId}. Asortyment nie został dodany do zlecenia.", result.Message);
			Assert.Equal(productId, result.ProductId);
			Assert.Equal(issueItem.Quantity, result.QuantityRequest);
			var stock = 30;
			Assert.Equal(stock, result.QuantityOnStock);
		}
	}
}
