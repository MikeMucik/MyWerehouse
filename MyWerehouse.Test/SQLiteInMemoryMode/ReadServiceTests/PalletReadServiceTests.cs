using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence.ReadServices;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class PalletReadServiceTests : TestBase
	{
		private static readonly Guid PalletId1 =
			Guid.Parse("00000000-0001-1111-0000-000000000000");
		private static readonly Guid ProductId1 =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ArchivedPalletId =
			Guid.Parse("00000000-9000-1111-0000-000000000000");

		private readonly PalletReadService _palletReadService;

		public PalletReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_palletReadService = new PalletReadService(DbContext);
		}

		[Fact]
		public async Task GetPalletByIdFullInfoAsync_ShouldReturnCompleteProjection_WhenPalletExists()
		{
			// Act
			var result = await _palletReadService.GetPalletByIdFullInfoAsync(
				PalletId1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal("Q1000", result.PalletNumber);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			Assert.Equal(PalletStatus.Available, result.Status);
			Assert.Equal("2-1-3-4", result.LocationSnapShot);
			Assert.Equal(1, result.ReceiptNumber);
			Assert.Equal(2, result.IssueNumber);

			Assert.Equal(2, result.ProductsOnPallet.Count);
			var firstProduct = result.ProductsOnPallet.Single(p => p.ProductId == ProductId1);
			Assert.Equal("0987654321", firstProduct.ProductSKU);
			Assert.Equal("Test", firstProduct.ProductName);
			Assert.Equal(50, firstProduct.Quantity);
			Assert.Equal(new DateTime(2024, 2, 2), firstProduct.DateAdded);
			Assert.Equal(
				DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366)),
				firstProduct.BestBefore);

			Assert.Equal(2, result.PalletHistory.Count);
			var movement = result.PalletHistory.Single(history => history.Id == 1);
			Assert.Equal("Q1000", movement.PalletNumber);
			Assert.Null(movement.LocationSnapShotSource);
			Assert.Null(movement.LocationSnapShotDestination);
			Assert.Equal(ReasonForPallet.Moved, movement.Reason);
			Assert.Equal("TestUser", movement.PerformedBy);
			Assert.Equal(new DateTime(2025, 2, 2), movement.MovementDate);
			Assert.Equal(2, movement.HistoryPalletDetailsDTO.Count);
			Assert.Equal(
				100,
				movement.HistoryPalletDetailsDTO
					.Single(detail => detail.ProductId == ProductId1)
					.QuantityChange);
		}

		[Fact]
		public async Task GetPalletByIdFullInfoAsync_ShouldReturnNull_WhenPalletDoesNotExist()
		{
			// Act
			var result = await _palletReadService.GetPalletByIdFullInfoAsync(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetPalletByPalletNumberAsync_ShouldReturnPallet_WhenActivePalletExists()
		{
			// Act
			var result = await _palletReadService.GetPalletByPalletNumberAsync(
				"Q1000",
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(PalletId1, result.Id);
			Assert.Equal("Q1000", result.PalletNumber);
			Assert.Equal(PalletStatus.Available, result.Status);
		}

		[Fact]
		public async Task GetPalletByPalletNumberAsync_ShouldReturnNull_WhenPalletIsArchived()
		{
			// Arrange
			DbContext.Pallets.Add(CreateArchivedPallet());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var result = await _palletReadService.GetPalletByPalletNumberAsync(
				"Q0000",
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task ShowPalletToEditAsync_ShouldReturnCompleteProjection_WhenActivePalletExists()
		{
			// Act
			var result = await _palletReadService.ShowPalletToEditAsync(
				PalletId1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(PalletId1, result.Id);
			Assert.Equal("Q1000", result.PalletNumber);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			Assert.Equal(1, result.LocationId);
			Assert.Equal("2-1-3-4", result.LocationSnapShot);
			Assert.Equal(PalletStatus.Available, result.Status);

			Assert.Equal(2, result.ProductsOnPallet.Count);
			var firstProduct = result.ProductsOnPallet.Single(p => p.ProductId == ProductId1);
			Assert.Equal("Test", firstProduct.ProductName);
			Assert.Equal("0987654321", firstProduct.SKU);
			Assert.Equal(new DateTime(2024, 2, 2), firstProduct.DateAdded);
			Assert.Equal(50, firstProduct.Quantity);
			Assert.Equal(
				DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366)),
				firstProduct.BestBefore);
		}

		[Fact]
		public async Task ShowPalletToEditAsync_ShouldReturnNull_WhenPalletIsArchived()
		{
			// Arrange
			DbContext.Pallets.Add(CreateArchivedPallet());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var result = await _palletReadService.ShowPalletToEditAsync(
				ArchivedPalletId,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldReturnPalletsContainingProduct()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				ProductId = ProductId1
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(4, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001", "Q1200", "Q5000"],
				result.Items.Select(pallet => pallet.PalletNumber));
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyProductAddedDateRange()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 3, 3)
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(7, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001", "Q1002", "Q1010", "Q1100", "Q1101", "Q2000"],
				result.Items.Select(pallet => pallet.PalletNumber));
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyBestBeforeRange()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				BestBeforeFrom = DateOnly.FromDateTime(TestDates.TodayDateTime.AddMonths(1))
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(9, result.TotalCount);
			Assert.Contains(result.Items, pallet => pallet.PalletNumber == "Q1000");
			Assert.Contains(result.Items, pallet => pallet.PalletNumber == "Q1010");
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyLocationBayFilter()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				LocationBay = 2
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001"],
				result.Items.Select(pallet => pallet.PalletNumber));
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldReturnEmptyPage_WhenLocationDoesNotMatch()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				LocationAisle = 3
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyIncomingClientFilter()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				ClientIdIn = 10
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001"],
				result.Items.Select(pallet => pallet.PalletNumber));
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyOutgoingClientFilter()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				ClientIdOut = 11
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(3, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001", "Q2000"],
				result.Items.Select(pallet => pallet.PalletNumber));
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyReceiptUserFilter()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				ReceiptUser = "U001"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001"],
				result.Items.Select(pallet => pallet.PalletNumber));
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyStatusFilter()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				PalletStatus = PalletStatus.Damaged
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			var pallet = Assert.Single(result.Items);
			Assert.Equal("Q1010", pallet.PalletNumber);
			Assert.Equal(PalletStatus.Damaged, pallet.Status);
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldApplyRemainingTextAndLocationFilters()
		{
			// Arrange
			var filter = new PalletSearchFilter
			{
				PalletNumber = "Q1000",
				ProductName = "testd",
				SKU = "fghtredfg",
				LocationBay = 2,
				LocationAisle = 1,
				LocationPosition = 3,
				LocationHeight = 4,
				IssueUser = "U002"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			var pallet = Assert.Single(result.Items);
			Assert.Equal(PalletId1, pallet.Id);
			Assert.Equal("Q1000", pallet.PalletNumber);
		}

		[Fact]
		public async Task GetPalletsByFilterAsync_ShouldPageInPalletNumberOrderAndExcludeArchivedPallets()
		{
			// Arrange
			DbContext.Pallets.Add(CreateArchivedPallet());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var result = await _palletReadService.GetPalletsByFilterAsync(
				new PalletSearchFilter(),
				currentPage: 1,
				pageSize: 3,
				CancellationToken.None);

			// Assert
			Assert.Equal(9, result.TotalCount);
			Assert.Equal(1, result.CurrentPage);
			Assert.Equal(3, result.PageSize);
			Assert.Equal(
				["Q1000", "Q1001", "Q1002"],
				result.Items.Select(pallet => pallet.PalletNumber));
			Assert.DoesNotContain(result.Items, pallet => pallet.Id == ArchivedPalletId);
		}

		private Task<PagedResult<PalletSimplyDTO>> FindAsync(
			PalletSearchFilter filter)
			=> _palletReadService.GetPalletsByFilterAsync(
				filter,
				currentPage: 1,
				pageSize: 20,
				CancellationToken.None);

		private static Pallet CreateArchivedPallet()
			=> Pallet.CreateForSeed(
				ArchivedPalletId,
				"Q0000",
				new DateTime(2025, 1, 1),
				locationId: 1,
				PalletStatus.Archived,
				receiptId: null,
				issueId: null);
	}
}
