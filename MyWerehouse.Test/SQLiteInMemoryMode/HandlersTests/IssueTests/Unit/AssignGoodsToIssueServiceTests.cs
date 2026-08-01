using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.IssueTests.Unit
{
	public class AssignGoodsToIssueServiceTests : TestBase
	{
		private const string UserId = "U001";
		private static readonly DateOnly BestBefore = new(2027, 6, 30);

		private readonly Mock<IAddPickingTaskToIssueService> _addPickingTaskToIssueService = new();
		private readonly Mock<IGetProductCountService> _getProductCountService = new();
		private readonly Mock<IVirtualPalletRepo> _virtualPalletRepo = new();
		private readonly Mock<IProductRepo> _productRepo = new();
		private readonly Mock<IPalletRepo> _palletRepo = new();

		[Fact]
		public async Task FullPalletIsAvailable_AssignGoodsToIssue_ReturnsSuccessAndReservesPallet()
		{
			//Arrange
			var product = CreateProduct(100);
			var issue = CreateIssue();
			var issueItem = CreateIssueItem(product.Id, 100);
			var pallet = CreateTrackedPallet(product.Id, 100);
			var service = CreateService();

			_productRepo
				.Setup(x => x.GetProductByIdAsync(product.Id))
				.ReturnsAsync(product);
			_getProductCountService
				.Setup(x => x.GetProductCountAsync(product.Id, BestBefore))
				.ReturnsAsync(100);
			_palletRepo
				.Setup(x => x.GetMissingFullPallets(product.Id, product.CartonsPerPallet, BestBefore, 1))
				.ReturnsAsync(new List<Pallet> { pallet });
			_virtualPalletRepo
				.Setup(x => x.GetVirtualPalletsByBBAsync(product.Id, BestBefore))
				.ReturnsAsync(new List<VirtualPallet>());

			//Act
			var result = await service.AssignGoodsToIssue(
				issue,
				issueItem,
				IssueAllocationPolicy.FullPalletFirst,
				null,
				UserId);

			//Assert
			Assert.True(result.Success);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(product.SKU, result.SKU);
			Assert.Single(result.AssignedPallets);
			Assert.Same(pallet, result.AssignedPallets.Single());
			Assert.Equal(PalletStatus.LockedForIssue, pallet.Status);
			Assert.Equal(issue.Id, pallet.IssueId);
			_addPickingTaskToIssueService.Verify(
				x => x.AddPickingTasksToIssue(
					It.IsAny<List<Pallet>?>(),
					It.IsAny<List<VirtualPallet>?>(),
					It.IsAny<Issue>(),
					It.IsAny<Guid>(),
					It.IsAny<int>(),
					It.IsAny<DateOnly?>(),
					It.IsAny<string>()),
				Times.Never);
		}

		[Fact]
		public async Task ProductDoesNotExist_AssignGoodsToIssue_ReturnsFailure()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var issue = CreateIssue();
			var issueItem = CreateIssueItem(productId, 100);
			var service = CreateService();

			_productRepo
				.Setup(x => x.GetProductByIdAsync(productId))
				.ReturnsAsync((Product?)null);

			//Act
			var result = await service.AssignGoodsToIssue(
				issue,
				issueItem,
				IssueAllocationPolicy.FullPalletFirst,
				null,
				UserId);

			//Assert
			Assert.False(result.Success);
			Assert.Equal(productId, result.ProductId);
			Assert.Equal("The specified product does not exist.", result.Message);
		}

		[Fact]
		public async Task RequestedQuantityExceedsStock_AssignGoodsToIssue_ReturnsFailureWithQuantities()
		{
			//Arrange
			var product = CreateProduct(100);
			var issue = CreateIssue();
			var issueItem = CreateIssueItem(product.Id, 100);
			var service = CreateService();

			_productRepo
				.Setup(x => x.GetProductByIdAsync(product.Id))
				.ReturnsAsync(product);
			_getProductCountService
				.Setup(x => x.GetProductCountAsync(product.Id, BestBefore))
				.ReturnsAsync(80);

			//Act
			var result = await service.AssignGoodsToIssue(
				issue,
				issueItem,
				IssueAllocationPolicy.FullPalletFirst,
				null,
				UserId);

			//Assert
			Assert.False(result.Success);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(product.SKU, result.SKU);
			Assert.Equal(100, result.QuantityRequest);
			Assert.Equal(80, result.QuantityOnStock);
			Assert.Contains("Insufficient quantity", result.Message);
		}

		[Fact]
		public async Task AllocationPolicyIsNotSupported_AssignGoodsToIssue_ReturnsFailure()
		{
			//Arrange
			var product = CreateProduct(100);
			var issue = CreateIssue();
			var issueItem = CreateIssueItem(product.Id, 100);
			var unsupportedPolicy = (IssueAllocationPolicy)999;
			var service = CreateService();

			_productRepo
				.Setup(x => x.GetProductByIdAsync(product.Id))
				.ReturnsAsync(product);
			_getProductCountService
				.Setup(x => x.GetProductCountAsync(product.Id, BestBefore))
				.ReturnsAsync(100);

			//Act
			var result = await service.AssignGoodsToIssue(
				issue,
				issueItem,
				unsupportedPolicy,
				null,
				UserId);

			//Assert
			Assert.False(result.Success);
			Assert.Equal($"Allocation policy {unsupportedPolicy} is not supported.", result.Message);
		}

		[Fact]
		public async Task SelectedPalletContainsMoreThanRequested_AssignGoodsToIssue_ReturnsFailure()
		{
			//Arrange
			var product = CreateProduct(100);
			var issue = CreateIssue();
			var issueItem = CreateIssueItem(product.Id, 100);
			var oversizedPallet = CreatePallet(product.Id, 150);
			var service = CreateService();

			_productRepo
				.Setup(x => x.GetProductByIdAsync(product.Id))
				.ReturnsAsync(product);
			_getProductCountService
				.Setup(x => x.GetProductCountAsync(product.Id, BestBefore))
				.ReturnsAsync(150);

			//Act
			var result = await service.AssignGoodsToIssue(
				issue,
				issueItem,
				IssueAllocationPolicy.FullPalletFirst,
				new List<Pallet> { oversizedPallet },
				UserId);

			//Assert
			Assert.False(result.Success);
			Assert.Equal("Allocated more product than requested.", result.Message);
			_virtualPalletRepo.Verify(
				x => x.GetVirtualPalletsByBBAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>()),
				Times.Never);
		}

		[Fact]
		public async Task PickingTaskCannotBeCreated_AssignGoodsToIssue_ReturnsFailure()
		{
			//Arrange
			var product = CreateProduct(100);
			var issue = CreateIssue();
			var issueItem = CreateIssueItem(product.Id, 50);
			var service = CreateService();
			const string pickingFailure = "Picking task cannot be created.";

			_productRepo
				.Setup(x => x.GetProductByIdAsync(product.Id))
				.ReturnsAsync(product);
			_getProductCountService
				.Setup(x => x.GetProductCountAsync(product.Id, BestBefore))
				.ReturnsAsync(50);
			_virtualPalletRepo
				.Setup(x => x.GetVirtualPalletsByBBAsync(product.Id, BestBefore))
				.ReturnsAsync(new List<VirtualPallet>());
			_addPickingTaskToIssueService
				.Setup(x => x.AddPickingTasksToIssue(
					It.IsAny<List<Pallet>?>(),
					It.IsAny<List<VirtualPallet>?>(),
					issue,
					product.Id,
					50,
					BestBefore,
					UserId))
				.ReturnsAsync(AddPickingTaskToIssueResult.Fail(pickingFailure));

			//Act
			var result = await service.AssignGoodsToIssue(
				issue,
				issueItem,
				IssueAllocationPolicy.FullPalletFirst,
				null,
				UserId);

			//Assert
			Assert.False(result.Success);
			Assert.Equal(pickingFailure, result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(product.SKU, result.SKU);
			Assert.Equal(50, result.QuantityRequest);
			Assert.Equal(50, result.QuantityOnStock);
		}

		private AssignProductToIssueAsyncService CreateService()
		{
			return new AssignProductToIssueAsyncService(
				_addPickingTaskToIssueService.Object,
				_getProductCountService.Object,
				_virtualPalletRepo.Object,
				_productRepo.Object,
				_palletRepo.Object);
		}

		private static Product CreateProduct(int cartonsPerPallet)
		{
			return Product.CreateForTests(
				Guid.NewGuid(),
				"Test product",
				"SKU-001",
				TestDates.UtcNow,
				1,
				false,
				cartonsPerPallet);
		}

		private static Issue CreateIssue()
		{
			return Issue.Create(
				1,
				1,
				new DateOnly(2027, 7, 10),
				TestDates.UtcNow,
				UserId);
		}

		private static IssueItemDTO CreateIssueItem(Guid productId, int quantity)
		{
			return new IssueItemDTO
			{
				ProductId = productId,
				Quantity = quantity,
				BestBefore = BestBefore
			};
		}

		private static Pallet CreatePallet(Guid productId, int quantity)
		{
			var pallet = Pallet.CreateForTests(
				"P1000",
				TestDates.UtcNow,
				1,
				PalletStatus.Available,
				null,
				null);
			pallet.AddProductForTests(productId, quantity, TestDates.UtcNow, BestBefore);
			return pallet;
		}

		private Pallet CreateTrackedPallet(Guid productId, int quantity)
		{
			var location = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Position = 1,
				Height = 1
			};
			var pallet = CreatePallet(productId, quantity);

			DbContext.Attach(location);
			DbContext.Attach(pallet);
			DbContext.Entry(pallet).Reference(x => x.Location).CurrentValue = location;

			return pallet;
		}
	}
}
