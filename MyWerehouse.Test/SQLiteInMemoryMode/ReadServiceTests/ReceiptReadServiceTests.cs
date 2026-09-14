using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Server.ServicesToInfrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class ReceiptReadServiceTests : TestBase
	{
		private static readonly Guid ReceiptId1 =
			Guid.Parse("11111111-1111-1111-1111-111111111111");
		private static readonly Guid ReceiptId2 =
			Guid.Parse("21111111-1111-1111-1111-111111111111");
		private static readonly Guid ProductId1 =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ProductNotInitiallyAssignedId =
			Guid.Parse("00000000-0000-0000-0989-000000000000");
		private static readonly Guid CancelledReceiptId =
			Guid.Parse("31111111-1111-1111-1111-111111111111");
		private static readonly Guid DeletedReceiptId =
			Guid.Parse("41111111-1111-1111-1111-111111111111");

		private readonly ReceiptReadService _receiptReadService;

		public ReceiptReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_receiptReadService = new ReceiptReadService(DbContext);
		}

		[Fact]
		public async Task GetReceiptById_ShouldReturnCompleteProjection_WhenReceiptExists()
		{
			// Act
			var result = await _receiptReadService.GetReceiptById(
				ReceiptId1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ReceiptId1, result.ReceiptId);
			Assert.Equal(1, result.ReceiptNumber);
			Assert.Equal(10, result.ClientId);
			Assert.Equal("ClientTest", result.ClientName);
			Assert.Equal(new DateTime(2023, 3, 3), result.ReceiptDateTime);
			Assert.Equal("U001", result.PerformedBy);
			Assert.Equal(1, result.RampNumber);
			Assert.Equal(ReceiptStatus.Verified, result.ReceiptStatus);

			Assert.Equal(2, result.Pallets.Count);
			var pallet = result.Pallets.Single(p => p.PalletNumber == "Q1000");
			Assert.Equal(
				Guid.Parse("00000000-0001-1111-0000-000000000000"),
				pallet.Id);
			Assert.Equal(new DateTime(2020, 1, 1), pallet.DateReceived);
			Assert.Equal(1, pallet.LocationId);
			Assert.Equal(2, pallet.ProductsOnPallet.Count);

			var product = pallet.ProductsOnPallet
				.Single(item => item.ProductId == ProductId1);
			Assert.Equal("0987654321", product.ProductSKU);
			Assert.Equal("Test", product.ProductName);
			Assert.Equal(50, product.Quantity);
			Assert.Equal(new DateTime(2024, 2, 2), product.DateAdded);
			Assert.Equal(TestDates.Today.AddDays(366), product.BestBefore);
		}

		[Fact]
		public async Task GetReceiptById_ShouldReturnNull_WhenReceiptDoesNotExist()
		{
			// Act
			var result = await _receiptReadService.GetReceiptById(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyReceiptNumberFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ReceiptNumber = 1
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			var receipt = Assert.Single(result.Items);
			Assert.Equal(ReceiptId1, receipt.ReceiptId);
			Assert.Equal(1, receipt.ReceiptNumber);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyClientIdFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = 10
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId1, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyClientNameFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ClientName = "ClientTest1"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId2, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyProductIdFilter()
		{
			// Arrange
			await AssignInitiallyUnusedProductToFirstReceiptAsync();
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = ProductNotInitiallyAssignedId
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId1, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyProductNameFilter()
		{
			// Arrange
			await AssignInitiallyUnusedProductToFirstReceiptAsync();
			var filter = new IssueReceiptSearchFilter
			{
				ProductName = "NotAdded"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId1, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplySkuFilter()
		{
			// Arrange
			await AssignInitiallyUnusedProductToFirstReceiptAsync();
			var filter = new IssueReceiptSearchFilter
			{
				SKU = "fghtredfg1"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId1, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyCreateDateRange()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				CreateDateStart = new DateTime(2023, 3, 1),
				CreateDateEnd = new DateTime(2023, 3, 31)
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId1, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldApplyUserFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				UserId = "U002"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(ReceiptId2, Assert.Single(result.Items).ReceiptId);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldReturnEmptyPage_WhenFilterDoesNotMatch()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ReceiptNumber = 999
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldPageInReceiptNumberOrderAndExcludeInactiveReceipts()
		{
			// Arrange
			DbContext.Receipts.AddRange(
				Receipt.CreateForSeed(
					CancelledReceiptId,
					receiptNumber: 3,
					clientId: 10,
					"U001",
					new DateTime(2023, 5, 5),
					ReceiptStatus.Cancelled,
					rampNumber: 1),
				Receipt.CreateForSeed(
					DeletedReceiptId,
					receiptNumber: 4,
					clientId: 10,
					"U001",
					new DateTime(2023, 6, 6),
					ReceiptStatus.Deleted,
					rampNumber: 1));
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var firstPage = await _receiptReadService.GetReceiptsByFilter(
				new IssueReceiptSearchFilter(),
				pageNumber: 1,
				pageSize: 1,
				CancellationToken.None);
			var secondPage = await _receiptReadService.GetReceiptsByFilter(
				new IssueReceiptSearchFilter(),
				pageNumber: 2,
				pageSize: 1,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, firstPage.TotalCount);
			Assert.Equal(1, firstPage.CurrentPage);
			Assert.Equal(1, firstPage.PageSize);
			Assert.Equal(ReceiptId1, Assert.Single(firstPage.Items).ReceiptId);

			Assert.Equal(2, secondPage.TotalCount);
			Assert.Equal(2, secondPage.CurrentPage);
			Assert.Equal(ReceiptId2, Assert.Single(secondPage.Items).ReceiptId);
			Assert.DoesNotContain(
				firstPage.Items.Concat(secondPage.Items),
				receipt => receipt.ReceiptId == CancelledReceiptId ||
					receipt.ReceiptId == DeletedReceiptId);
		}

		private Task<PagedResult<ReceiptSimplyDTO>> FindAsync(
			IssueReceiptSearchFilter filter)
			=> _receiptReadService.GetReceiptsByFilter(
				filter,
				pageNumber: 1,
				pageSize: 20,
				CancellationToken.None);

		private async Task AssignInitiallyUnusedProductToFirstReceiptAsync()
		{
			var pallet = DbContext.Pallets.Single(p => p.PalletNumber == "Q1000");
			pallet.AddProduct(
				ProductNotInitiallyAssignedId,
				quantity: 25,
				new DateTime(2024, 5, 5),
				bestBefore: null);
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();
		}
	}
}
