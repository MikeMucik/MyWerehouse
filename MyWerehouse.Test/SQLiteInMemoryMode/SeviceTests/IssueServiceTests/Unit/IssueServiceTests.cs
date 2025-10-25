using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using static MyWerehouse.Application.ViewModels.IssueModels.CreateIssueDTO;
using static MyWerehouse.Application.ViewModels.IssueModels.UpdateIssueDTO;

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
			var historyServiceMock = new Mock<IHistoryService>();
			var inventoryServiceMock = new Mock<IInventoryService>();
			var productRepoMock = new Mock<IProductRepo>();
			var palletRepo = new Mock<IPalletRepo>();
			var allocationRepo = new Mock<IAllocationRepo>();
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var palletService = new Mock<IPalletService>();
			var issueItem = new Mock< IIssueItemRepo>();
		var validator = new Mock<IValidator<CreateIssueDTO>>();
		var updateIssueValidator = new Mock<IValidator<UpdateIssueDTO>>();

			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				historyServiceMock.Object,
				inventoryServiceMock.Object,
				palletRepo.Object,
				productRepoMock.Object,
				allocationRepo.Object,
				pickingRepoMock.Object,
				palletService.Object,
				issueItem.Object,
				validator.Object,
				updateIssueValidator.Object
			);

			// Act
			await service.FinishIssueNotCompleted(issueId, performedBy);

			// Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(PalletStatus.Available, notLoadedPallet.Status);
			Assert.Null(notLoadedPallet.IssueId);

			historyServiceMock.Verify(x =>
				x.CreateOperation(notLoadedPallet, 20, ReasonMovement.Correction, performedBy, PalletStatus.Available, null), Times.Once);

			historyServiceMock.Verify(x =>
				x.CreateOperation(loadedPallet, 10, ReasonMovement.Loaded, performedBy, PalletStatus.Loaded, null), Times.Once);

			inventoryServiceMock.Verify(x =>
				x.ChangeProductQuantityAsync(101, -5), Times.Once);

			historyServiceMock.Verify(x =>
				x.CreateHistoryIssue(issue //IssueStatus.IsShipped, null), Times.Once
												 ))
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
				IssueStatus = IssueStatus.InProgress,
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

			// Mocky
			var movementServiceMock = new Mock<IHistoryService>();
			//var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var allocationRepo = new AllocationRepo(DbContext);
			var pickingRepo = new PickingPalletRepo(DbContext);
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id))
						 .ReturnsAsync(issue);
			var inventoryRepo = new InventoryRepo(DbContext);
			
			var mockMapper = new Mock<IMapper>();
			movementServiceMock
				.Setup(r => r.CreateOperation(It.IsAny<Pallet>(), It.IsAny<int>(), It.IsAny<ReasonMovement>(), It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				;
			var inventoryService = new InventoryService(inventoryRepo, mockMapper.Object, DbContext );
			var palletRepo = new PalletRepo(DbContext);
			var productRepo = new ProductRepo(DbContext);
			var locationRepo = new LocationRepo(DbContext);
			var palletMovementRepo = new PalletMovementRepo(DbContext);
			var historyIssueRepo = new HistoryIssueRepo(DbContext);
			var historyReceiptRepo = new HistoryReceiptRepo(DbContext);
			var historyAllocationRepo = new HistoryPickingRepo(DbContext);
			var historyService = new HistoryService(palletMovementRepo, historyIssueRepo, historyReceiptRepo, historyAllocationRepo, DbContext, palletRepo, mockMapper.Object, locationRepo);

			var locationService = new Mock<ILocationService>();

			var productOnPalletValidator = new ProductOnPalletDTOValidation();
			var updatePalletValidator = new UpdatePalletDTOValidation(productOnPalletValidator);
			var palletService = new PalletService(palletRepo,
				historyService,
				locationService.Object,
				palletMovementRepo,
				pickingRepo,
				locationRepo,
				mockMapper.Object,
				updatePalletValidator, DbContext);
			var itemIssue = new Mock<IIssueItemRepo>();
			var validatorItem = new IssueItemDTOValidion();
			var validator = new CreateIssueDTOValidion(validatorItem);
			var validatorUpdate = new UpdateIssueDTOValidation(validatorItem);
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				movementServiceMock.Object,
				inventoryService,
				palletRepo,
				productRepo,
				allocationRepo,
				pickingRepo,
				palletService,
				itemIssue.Object,
				validator,
				validatorUpdate
				);

			// Act
			await service.AddPalletsToIssueByProductAsync(issue, issueItem);
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count);
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));

			//pickingRepoMock.Verify(r => r.AddAllocationAsync(It.IsAny<int>(), issue.Id, 5), Times.Once);
			movementServiceMock.Verify(r => r.CreateOperation(It.IsAny<Pallet>(), It.IsAny<int>(), ReasonMovement.ToLoad, It.IsAny<string>(), It.IsAny<PalletStatus>(), null), Times.Exactly(2));

			movementServiceMock.Verify(m => m.CreateOperation(
				It.Is<Pallet>(p => p.Id == "P1"), 1, ReasonMovement.ToLoad, It.IsAny<string>(), PalletStatus.InTransit, null), Times.Once);

			//pickingRepoMock.Verify(p => p.AddAllocationAsync(1, issue.Id, 5), Times.Once);

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
				IssueStatus = IssueStatus.InProgress,
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

			// Mocki
			var movementServiceMock = new Mock<IHistoryService>();
			movementServiceMock
				.Setup(r => r.CreateOperation(It.IsAny<Pallet>(), It.IsAny<int>(), It.IsAny<ReasonMovement>(), It.IsAny<string>(), It.IsAny<PalletStatus>(), null));
				
			var allocationRepo = new AllocationRepo(DbContext);
			var pickingRepo = new PickingPalletRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);
			var productRepo = new ProductRepo(DbContext);
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id)).ReturnsAsync(issue);
			var inventoryRepo = new InventoryRepo(DbContext);
			var mockMapper = new Mock<IMapper>();
			var inventoryService = new InventoryService(inventoryRepo, mockMapper.Object, DbContext);
			var palletMovementRepo = new PalletMovementRepo(DbContext);
			var locationRepo = new LocationRepo(DbContext);
			var historyIssueRepo = new HistoryIssueRepo(DbContext);
			var historyReceiptRepo = new HistoryReceiptRepo(DbContext);
			var historyAllocationRepo = new HistoryPickingRepo(DbContext);
			var historyService = new HistoryService(palletMovementRepo, historyIssueRepo, historyReceiptRepo, historyAllocationRepo, DbContext, palletRepo, mockMapper.Object, locationRepo);
			var productOnPalletValidator = new ProductOnPalletDTOValidation();
			var updatePalletValidator = new UpdatePalletDTOValidation(productOnPalletValidator);
			var locationService = new Mock<ILocationService>();
			var palletService = new PalletService(palletRepo,
				historyService,
				locationService.Object,
				palletMovementRepo,
				pickingRepo,
				locationRepo,
				mockMapper.Object,
				updatePalletValidator, DbContext);
			var itemIssue = new Mock<IIssueItemRepo>();
			var validatorItem = new IssueItemDTOValidion();
			var validator = new CreateIssueDTOValidion(validatorItem);
			var validatorUpdate = new UpdateIssueDTOValidation(validatorItem);

			//var palletService = new Mock<IPalletService>();
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				movementServiceMock.Object,
				inventoryService,
				palletRepo,
				productRepo,
				allocationRepo,
				pickingRepo,
				palletService,
				itemIssue.Object,
				validator,
				validatorUpdate
			);

			// Act
			await service.AddPalletsToIssueByProductAsync(issue, issueItem);
			await DbContext.SaveChangesAsync();

			// Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));

			movementServiceMock.Verify(r =>
				r.CreateOperation(It.IsAny<Pallet>(), It.IsAny<int>(), ReasonMovement.ToLoad, It.IsAny<string>(), It.IsAny<PalletStatus>(), null), Times.Exactly(2));

			movementServiceMock.Verify(m =>
				m.CreateOperation(It.Is<Pallet>(p => p.Id == "P1"), 1, ReasonMovement.ToLoad, It.IsAny<string>(), PalletStatus.InTransit, null), Times.Once);
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
				IssueStatus = IssueStatus.InProgress,
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

			// Mocki
			var movementServiceMock = new Mock<IHistoryService>();
			movementServiceMock
				.Setup(r => r.CreateOperation(It.IsAny<Pallet>(), It.IsAny<int>(), It.IsAny<ReasonMovement>(), It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				;
			var allocationRepo = new AllocationRepo(DbContext);
			var pickingRepo = new PickingPalletRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);
			var productRepo = new ProductRepo(DbContext);
			var issueRepoMock = new Mock<IIssueRepo>();
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(issue.Id)).ReturnsAsync(issue);
			var inventoryRepo = new InventoryRepo(DbContext);
			var mockMapper = new Mock<IMapper>();
			var inventory = new InventoryService(inventoryRepo, mockMapper.Object, DbContext);
			var palletService = new Mock<IPalletService>();
			var itemIssue = new Mock<IIssueItemRepo>();
			var validator = new Mock<IValidator<CreateIssueDTO>>();
			var validatorUpdate = new Mock<IValidator<UpdateIssueDTO>>();
			var service = new IssueService(
				issueRepoMock.Object,
				mockMapper.Object,
				DbContext,
				movementServiceMock.Object,
				inventory,
				palletRepo,
				productRepo,
				allocationRepo,
				pickingRepo,
				palletService.Object,
				itemIssue.Object,
				validator.Object,
				validatorUpdate.Object
			);

			//Act
			var result = await service.AddPalletsToIssueByProductAsync(issue, issueItem);
			//Assert
			Assert.False(result.Success);
			Assert.Contains("Produkt o numerze 100 nie został dodany do", result.Message);
			Assert.Equal(productId, result.ProductId);
			Assert.Equal(issueItem.Quantity, result.QuantityRequest);
			var stock = 30;
			Assert.Equal(stock, result.QuantityOnStock);
		}

	}
}
