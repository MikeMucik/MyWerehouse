using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

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
				IssueDateTime = new DateTime(2025, 6, 6, 2, 2, 2),
				Pallets = new List<Pallet> { loadedPallet, notLoadedPallet }
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.AddRange(initailLocation, initailLocation1);
			DbContext.Clients.Add(initailCLient);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			var mockMapper = new Mock<IMapper>();

			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issueId))
						 .ReturnsAsync(issue);

			var palletMovementServiceMock = new Mock<IPalletMovementService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var productRepoMock = new Mock<IProductRepo>();
			var palletRepo = new Mock<IPalletRepo>();
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				palletMovementServiceMock.Object,
				inventoryRepoMock.Object,
				palletRepo.Object,
				productRepoMock.Object,
				pickingRepoMock.Object
			);

			// Act
			await service.FinishIssueNotCompleted(issueId, performedBy);

			// Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(PalletStatus.Available, notLoadedPallet.Status);
			Assert.Null(notLoadedPallet.IssueId);

			palletMovementServiceMock.Verify(x =>
				x.CreateMovementAsync(notLoadedPallet, 20, ReasonMovement.Correction, performedBy, null), Times.Once);

			palletMovementServiceMock.Verify(x =>
				x.CreateMovementAsync(loadedPallet, 10, ReasonMovement.Loaded, performedBy, null), Times.Once);

			inventoryRepoMock.Verify(x =>
				x.DecreaseInventoryQuantityAsync(101, 5), Times.Once);

			palletMovementServiceMock.Verify(x =>
				x.CreateHistoryIssueAsync(issue, IssueStatus.IsShipped, performedBy, null), Times.Once);
			;
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
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_ShouldAssignFullPallets_WhenSufficientStock()
		{
			// Arrange
			var issue = new Issue { Id = 1, PerformedBy = "user" };
			var product = new IssueItemDTO { ProductId = 10, Quantity = 200, BestBefore = new DateOnly(2025, 8, 1) };
			var cartonsPerPallet = 100;
			var pallets = new List<Pallet>
	{
		new() { Id = "Q1000", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = cartonsPerPallet } }, LocationId = 1 },
		new() { Id = "Q1001", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = cartonsPerPallet } }, LocationId = 2 }
	};
			var mockMapper = new Mock<IMapper>();
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
						 .ReturnsAsync(issue);

			var palletMovementServiceMock = new Mock<IPalletMovementService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var productRepoMock = new Mock<IProductRepo>();
			var palletRepoMock = new Mock<IPalletRepo>();
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
				.Returns(pallets.AsQueryable());

			productRepoMock.Setup(r => r.GetProductByIdAsync(10))
				.ReturnsAsync(new Product { Id = 10, CartonsPerPallet = cartonsPerPallet });
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				palletMovementServiceMock.Object,
				inventoryRepoMock.Object,
				palletRepoMock.Object,
				productRepoMock.Object,
				pickingRepoMock.Object
			);
			// Act
			await service.AddPalletsToIssueByProductAsync(issue, product, "user");

			// Assert
			Assert.Equal(2, issue.Pallets.Count);
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
		}
		[Fact]
		public async Task AddPalletsToIssueByProductAsync_ShouldThrow_WhenInsufficientStock()
		{
			// Arrange
			var issue = new Issue { Id = 1 };
			var product = new IssueItemDTO { ProductId = 10, Quantity = 300, BestBefore = new DateOnly(2025, 8, 1) };
			var pallets = new List<Pallet>
	{
		new() { Id = "Q1000", ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } } },
		new() { Id = "Q1001", ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } } }
	};
			var mockMapper = new Mock<IMapper>();
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
						 .ReturnsAsync(issue);

			var palletMovementServiceMock = new Mock<IPalletMovementService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var productRepoMock = new Mock<IProductRepo>();
			var palletRepoMock = new Mock<IPalletRepo>();
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
				.Returns(pallets.AsQueryable());
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				palletMovementServiceMock.Object,
				inventoryRepoMock.Object,
				palletRepoMock.Object,
				productRepoMock.Object,
				pickingRepoMock.Object
			);
			// Act & Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				service.AddPalletsToIssueByProductAsync(issue, product, "user"));

			Assert.Contains("Brak wystarczającej ilości", ex.Message);
		}
		//	[Fact]
		//	public async Task AddPalletsToIssueByProductAsync_ShouldAddFullAndPicking_WhenPartialNeeded()
		//	{
		//		// Arrange
		//		var issue = new Issue { Id = 1, PerformedBy = "user" };
		//		var product = new IssueItemDTO { ProductId = 10, Quantity = 250, BestBefore = new DateOnly(2025, 8, 1) };
		//		var cartonsPerPallet = 100;

		//		var pallets = new List<Pallet>
		//{
		//	new() { Id = "Q1000", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } }, LocationId = 1 },
		//	new() { Id = "Q1001", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } }, LocationId = 2 },
		//	new() { Id = "Q1002", Status = PalletStatus.Available, ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 50 } }, LocationId = 3 }
		//};
		//		var mockMapper = new Mock<IMapper>();
		//		var issueRepoMock = new Mock<IIssueRepo>();
		//		issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
		//					 .ReturnsAsync(issue);

		//		var palletMovementServiceMock = new Mock<IPalletMovementService>();
		//		var inventoryRepoMock = new Mock<IInventoryRepo>();
		//		var productRepoMock = new Mock<IProductRepo>();
		//		var palletRepoMock = new Mock<IPalletRepo>();
		//		var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//		palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
		//			.Returns(pallets.AsQueryable());

		//		productRepoMock.Setup(r => r.GetProductByIdAsync(10))
		//			.ReturnsAsync(new Product { Id = 10, CartonsPerPallet = cartonsPerPallet });

		//		// śledzimy czy alokacja została wywołana
		//		bool allocationCalled = false;
		//		serviceMock.Setup(s => s.AddAllocationToIssue(It.IsAny<string>(), It.IsAny<i>(), It.Is<int>(q => q == 50), It.IsAny<DateOnly>(), It.IsAny<string>()))
		//			.Callback(() => allocationCalled = true)
		//			.Returns(Task.CompletedTask);
		//		var service = new IssueService(
		//			issueRepoMock.Object,
		//			mockMapper.Object,
		//			DbContext,
		//			palletMovementServiceMock.Object,
		//			inventoryRepoMock.Object,
		//			palletRepoMock.Object,
		//			productRepoMock.Object,
		//			pickingRepoMock.Object
		//		);
		//		// Act
		//		await service.AddPalletsToIssueByProductAsync(issue, product, "user");

		//		// Assert
		//		Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety
		//		Assert.True(allocationCalled);
		//	}
		//	[Fact]
		//	public async Task AddPalletsToIssueByProductAsync_ShouldAddFullAndPicking_WhenPartialNeeded()
		//	{
		//		// Arrange
		//		var issue = new Issue { Id = 1, PerformedBy = "user" };
		//		var product = new IssueItemDTO
		//		{
		//			ProductId = 10,
		//			Quantity = 250,
		//			BestBefore = new DateOnly(2025, 8, 1)
		//		};

		//		var cartonsPerPallet = 100;
		//		var pallets = new List<Pallet>
		//{
		//	new()
		//	{
		//		Id = "Q1000",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } },
		//		LocationId = 1
		//	},
		//	new()
		//	{
		//		Id = "Q1001",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } },
		//		LocationId = 2
		//	},
		//	new()
		//	{
		//		Id = "Q1002",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 50 } },
		//		LocationId = 3
		//	}
		//};

		//		var mockMapper = new Mock<IMapper>();
		//		var issueRepoMock = new Mock<IIssueRepo>();
		//		issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
		//			.ReturnsAsync(issue);

		//		var palletMovementServiceMock = new Mock<IPalletMovementService>();
		//		var inventoryRepoMock = new Mock<IInventoryRepo>();
		//		var productRepoMock = new Mock<IProductRepo>();
		//		var palletRepoMock = new Mock<IPalletRepo>();
		//		var pickingRepoMock = new Mock<IPickingPalletRepo>();

		//		palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
		//			.Returns(pallets.AsQueryable());

		//		productRepoMock.Setup(r => r.GetProductByIdAsync(10))
		//			.ReturnsAsync(new Product { Id = 10, CartonsPerPallet = cartonsPerPallet });

		//		// Śledzenie, czy AddAllocationToIssue zostało wywołane
		//		bool allocationCalled = false;
		//		pickingRepoMock.Setup(r =>
		//				r.AddAllocationAsync(
		//					It.IsAny<int>(),
		//					It.Is<string>(pid => pid == "Q1002"),
		//					It.Is<int>(q => q == 50)
		//					//It.IsAny<DateOnly>(),
		//					//It.Is<string>(u => u == "user")
		//					))
		//			.Callback(() => allocationCalled = true)
		//			.Returns(Task.CompletedTask);

		//		var service = new IssueService(
		//			issueRepoMock.Object,
		//			mockMapper.Object,
		//			DbContext,
		//			palletMovementServiceMock.Object,
		//			inventoryRepoMock.Object,
		//			palletRepoMock.Object,
		//			productRepoMock.Object,
		//			pickingRepoMock.Object
		//		);

		//		// Act
		//		await service.AddPalletsToIssueByProductAsync(issue, product, "user");

		//		// Assert
		//		Assert.Equal(2, issue.Pallets.Count); // dwie pełne palety
		//		Assert.True(allocationCalled);       // picking też został wywołany
		//		Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
		//	}
		//	[Fact]
		//	public async Task AddPalletsToIssueByProductAsync_ShouldAssignFullAndCallAllocation()
		//	{
		//		// Arrange
		//		var issue = new Issue { Id = 1, PerformedBy = "user", Pallets = new List<Pallet>() };
		//		var product = new IssueItemDTO
		//		{
		//			ProductId = 10,
		//			Quantity = 250, // 2 pełne + 50 do pickingu
		//			BestBefore = new DateOnly(2025, 8, 1)
		//		};
		//		var cartonsPerPallet = 100;
		//		var pallets = new List<Pallet>
		//{
		//	new() {
		//		Id = "Q1000",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } },
		//		LocationId = 1
		//	},
		//	new() {
		//		Id = "Q1001",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 100 } },
		//		LocationId = 1
		//	},
		//	new() {
		//		Id = "Q1002",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 10, Quantity = 50 } },
		//		LocationId = 1
		//	},
		//};

		//		var mockMapper = new Mock<IMapper>();
		//		var issueRepoMock = new Mock<IIssueRepo>();
		//		issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id)).ReturnsAsync(issue);

		//		var palletRepoMock = new Mock<IPalletRepo>();
		//		palletRepoMock.Setup(r => r.GetAvailablePallets(product.ProductId, product.BestBefore))
		//			.Returns(pallets.AsQueryable());

		//		var productRepoMock = new Mock<IProductRepo>();
		//		productRepoMock.Setup(r => r.GetProductByIdAsync(product.ProductId))
		//			.ReturnsAsync(new Product { Id = product.ProductId, CartonsPerPallet = 100 });

		//		var palletMovementServiceMock = new Mock<IPalletMovementService>();
		//		var inventoryRepoMock = new Mock<IInventoryRepo>();

		//		var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//		palletRepoMock.Setup(r => r.GetAvailablePallets(10, product.BestBefore))
		//			.Returns(pallets.AsQueryable());

		//		pickingRepoMock
		//			.Setup(r => r.AddAllocationAsync(100, 1, 50))
		//			.Returns(Task.CompletedTask)
		//			.Verifiable();

		//		var service = new IssueService(
		//			issueRepoMock.Object,
		//			mockMapper.Object,
		//			DbContext,
		//			palletMovementServiceMock.Object,
		//			inventoryRepoMock.Object,
		//			palletRepoMock.Object,
		//			productRepoMock.Object,
		//			pickingRepoMock.Object
		//		);

		//		// Act
		//		await service.AddPalletsToIssueByProductAsync(issue, product, "user");

		//		// Assert
		//		Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety po 100
		//		Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
		//		pickingRepoMock.Verify(r => r.AddAllocationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.AtLeastOnce);

		//		pickingRepoMock.Verify(r => r.GetPickingPalletsAsync(2), Times.AtLeastOnce);
		//		pickingRepoMock.Verify(r => r.AddPalletToPickingAsync("Q1002"), Times.Once);
		//	}
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
				IssueDateTime = DateTime.Now,

				IssueStatus = IssueStatus.InProgress,
				Pallets = new List<Pallet>()
			};
			DbContext.Clients.Add(initailCLient);
			DbContext.Locations.AddRange(new Location { Id = 1, Aisle = 1, Bay = 1, Height = 1, Position = 1 }, new Location { Id = 2, Aisle = 2, Bay = 1, Height = 1, Position = 1 });
			DbContext.Categories.Add(new Category { Id = 1, Name = "tested" });
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Products.Add(new Product { Id = productId, CartonsPerPallet = cartonsPerPallet, Name ="test", SKU = "123", CategoryId =1 });
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();

			// Mocky
			var movementServiceMock = new Mock<IPalletMovementService>();
			//var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var pickingRepo = new PickingPalletRepo(DbContext);
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
						 .ReturnsAsync(issue);
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			//pickingRepoMock
			//	.Setup(r => r.GetPickingPalletsAsync(productId))
			//	.ReturnsAsync(new List<PickingPallet>
			//	{
			//new PickingPallet
			//{
			//	Id = 1,
			//	PalletId = "P2",
			//	IssueInitialQuantity = 10,
			//	Allocation = new List<Allocation>()
			//}
			//	});


			//pickingRepoMock
			//	.Setup(r => r.GetPickingPalletsAsync(productId))
			//	.ReturnsAsync(new List<PickingPallet>()); // PUSTA lista

			var mockMapper = new Mock<IMapper>();
			//pickingRepoMock
			//	.Setup(r => r.AddAllocationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
			//	.Returns(Task.CompletedTask);

			//pickingRepoMock
			//	.Setup(r => r.AddPalletToPickingAsync(It.IsAny<string>()))
			//	.Returns(Task.CompletedTask);

			movementServiceMock
				.Setup(r => r.CreateMovementAsync(It.IsAny<Pallet>(), It.IsAny<int>(), It.IsAny<ReasonMovement>(), It.IsAny<string>(), null))
				.Returns(Task.CompletedTask);

			var palletRepo = new PalletRepo(DbContext);
			var productRepo = new ProductRepo(DbContext);

			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				movementServiceMock.Object,
				inventoryRepoMock.Object,
				palletRepo,
				productRepo,
				pickingRepo
				);

			// Act
			await service.AddPalletsToIssueByProductAsync(issue, issueItem, "user1");
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.Equal(IssueStatus.InProgress, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count);
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));

			//pickingRepoMock.Verify(r => r.AddAllocationAsync(It.IsAny<int>(), issue.Id, 5), Times.Once);
			movementServiceMock.Verify(r => r.CreateMovementAsync(It.IsAny<Pallet>(), It.IsAny<int>(), ReasonMovement.ToLoad, It.IsAny<string>(), null), Times.Exactly(2));

			movementServiceMock.Verify(m => m.CreateMovementAsync(
	It.Is<Pallet>(p => p.Id == "P1"), 1, ReasonMovement.ToLoad, "user1", null), Times.Once);

			//pickingRepoMock.Verify(p => p.AddAllocationAsync(1, issue.Id, 5), Times.Once);

		}


	}
}
