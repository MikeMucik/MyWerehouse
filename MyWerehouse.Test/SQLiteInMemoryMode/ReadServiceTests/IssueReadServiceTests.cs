using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFilter;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Server.ServicesToInfrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class IssueReadServiceTests : TestBase
	{
		private static readonly Guid IssueId =
			Guid.Parse("11111111-2111-1111-1111-111111111111");
		private static readonly Guid ProductId1 =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ProductId2 =
			Guid.Parse("00000000-0000-0000-0002-000000000000");
		private static readonly Guid PalletId1 =
			Guid.Parse("00000000-0001-1111-0000-000000000000");
		private static readonly Guid ArchivedIssueId =
			Guid.Parse("11111111-9000-1111-1111-111111111111");
		private static readonly Guid ArchivedPalletId =
			Guid.Parse("00000000-9000-1111-0000-000000000000");

		private readonly IssueReadService _issueReadService;

		public IssueReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_issueReadService = new IssueReadService(DbContext);
		}

		[Fact]
		public async Task GetIssueById_ShouldReturnCompleteProjection_WhenIssueExists()
		{
			// Act
			var result = await _issueReadService.GetIssueById(
				IssueId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(IssueId, result.Id);
			Assert.Equal(2, result.IssueNumber);
			Assert.Equal(11, result.ClientId);
			Assert.Equal("ClientTest1", result.ClientName);
			Assert.Equal(TestDates.UtcNow.AddDays(-5), result.IssueDateTimeCreate);
			Assert.Equal(TestDates.Today.AddDays(1), result.IssueDateTimeSend);
			Assert.Equal("U002", result.PerformedBy);
			Assert.Equal(IssueStatus.New, result.IssueStatus);

			Assert.Equal(3, result.Pallets.Count);
			var pallet = result.Pallets.Single(p => p.Id == PalletId1);
			Assert.Equal("Q1000", pallet.PalletNumber);
			Assert.Equal(1, pallet.LocationId);
			Assert.Equal("2-1-3-4", pallet.LocationSnapShot);
			Assert.Equal(PalletStatus.Available, pallet.Status);
			Assert.Equal(2, pallet.ProductsOnPallet.Count);

			var productOnPallet = pallet.ProductsOnPallet
				.Single(product => product.ProductId == ProductId1);
			Assert.Equal("Test", productOnPallet.ProductName);
			Assert.Equal("0987654321", productOnPallet.ProductSKU);
			Assert.Equal(50, productOnPallet.Quantity);
			Assert.Equal(new DateTime(2024, 2, 2), productOnPallet.DateAdded);
			Assert.Equal(TestDates.Today.AddDays(366), productOnPallet.BestBefore);

			Assert.Equal(2, result.IssueItems.Count);
			var issueItem = result.IssueItems
				.Single(item => item.ProductId == ProductId2);
			Assert.Equal("TestD", issueItem.ProductName);
			Assert.Equal("fghtredfg", issueItem.ProductSKU);
			Assert.Equal(400, issueItem.Quantity);
			Assert.Equal(TestDates.Today.AddMonths(3), issueItem.BestBefore);
		}

		[Fact]
		public async Task GetIssueById_ShouldReturnNull_WhenIssueIsArchived()
		{
			// Arrange
			await AddArchivedIssueAsync();

			// Act
			var result = await _issueReadService.GetIssueById(
				ArchivedIssueId,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyIssueNumberFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				IssueNumber = 2
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			var issue = Assert.Single(result.Items);
			Assert.Equal(IssueId, issue.Id);
			Assert.Equal(2, issue.IssueNumber);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyClientIdFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = 11
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyProductIdFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = ProductId1
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplySendDateRange()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				SendDateStart = TestDates.Today,
				SendDateEnd = TestDates.Today.AddDays(1)
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyClientNameFilter()
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
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyProductNameFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ProductName = "TestD"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplySkuFilter()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				SKU = "fghtredfg"
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyCreateDateRange()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				CreateDateStart = TestDates.UtcNow.AddDays(-6),
				CreateDateEnd = TestDates.UtcNow.AddDays(-4)
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(1, result.TotalCount);
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldApplyUserFilter()
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
			Assert.Equal(IssueId, Assert.Single(result.Items).Id);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldReturnEmptyPage_WhenFilterDoesNotMatch()
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				IssueNumber = 999
			};

			// Act
			var result = await FindAsync(filter);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		[Fact]
		public async Task GetIssueByFilter_ShouldPageInIssueNumberOrderAndExcludeArchivedIssues()
		{
			// Arrange
			DbContext.Issues.Add(CreateIssue(
				Guid.Parse("11111111-3000-1111-1111-111111111111"),
				issueNumber: 3,
				IssueStatus.New));
			DbContext.Issues.Add(CreateArchivedIssue());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var result = await _issueReadService.GetIssueByFilter(
				new IssueReceiptSearchFilter(),
				pageNumber: 1,
				pageSize: 1,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(1, result.CurrentPage);
			Assert.Equal(1, result.PageSize);
			var issue = Assert.Single(result.Items);
			Assert.Equal(IssueId, issue.Id);
			Assert.Equal(2, issue.IssueNumber);
			Assert.DoesNotContain(result.Items, item => item.Id == ArchivedIssueId);
		}

		[Fact]
		public async Task SummaryProductsIssue_ShouldReturnCompleteProjection_WhenIssueExists()
		{
			// Act
			var result = await _issueReadService.SummaryProductsIssue(
				IssueId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(IssueId, result.Id);
			Assert.Equal(2, result.IssueNumber);
			Assert.Equal(11, result.ClientId);
			Assert.Equal("U002", result.PerformedBy);
			Assert.Equal(TestDates.Today.AddDays(1), result.DateToSend);
			Assert.Equal(2, result.IssueItems.Count);

			var item = result.IssueItems.Single(i => i.ProductId == ProductId1);
			Assert.Equal("Test", item.ProductName);
			Assert.Equal("0987654321", item.ProductSKU);
			Assert.Equal(150, item.Quantity);
			Assert.Equal(TestDates.Today.AddMonths(3), item.BestBefore);
		}

		[Fact]
		public async Task SummaryProductsIssue_ShouldReturnNull_WhenIssueIsArchived()
		{
			// Arrange
			await AddArchivedIssueAsync();

			// Act
			var result = await _issueReadService.SummaryProductsIssue(
				ArchivedIssueId,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task ListPalletsToLoad_ShouldReturnOnlyPalletsAllowedForLoading_WhenIssueExists()
		{
			// Act
			var result = await _issueReadService.ListPalletsToLoad(
				IssueId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(IssueId, result.IssueId);
			Assert.Equal(2, result.IssueNumber);
			Assert.Equal(11, result.ClientId);
			Assert.Equal("ClientTest1", result.ClientName);
			Assert.Equal(["Q1000", "Q2000"], result.Pallets.Select(p => p.PalletNumber));
			Assert.DoesNotContain(result.Pallets, p => p.PalletNumber == "Q1001");

			var pallet = result.Pallets.Single(p => p.PalletNumber == "Q1000");
			Assert.Equal(PalletId1, pallet.PalletId);
			Assert.Equal(1, pallet.LocationId);
			Assert.Equal("2-1-3-4", pallet.LocationName);
			Assert.Equal(PalletStatus.Available, pallet.PalletStatus);
			Assert.Equal(2, pallet.ProductOnPalletIssue.Count);

			var product = pallet.ProductOnPalletIssue
				.Single(p => p.ProductId == ProductId2);
			Assert.Equal("TestD", product.ProductName);
			Assert.Equal("fghtredfg", product.SKU);
			Assert.Equal(200, product.Quantity);
			Assert.Equal(TestDates.Today.AddDays(366), product.BestBefore);
		}

		[Fact]
		public async Task ListPalletsToLoad_ShouldReturnNull_WhenIssueIsArchived()
		{
			// Arrange
			await AddArchivedIssueAsync();

			// Act
			var result = await _issueReadService.ListPalletsToLoad(
				ArchivedIssueId,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetPalletToTakeOff_ShouldReturnPalletsInLocationOrder_WhenIssueExists()
		{
			// Act
			var result = await _issueReadService.GetPalletToTakeOff(
				IssueId,
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(3, result.TotalCount);
			Assert.Equal(
				["Q1000", "Q1001", "Q2000"],
				result.Items.Select(pallet => pallet.PalletNumber));
			Assert.All(result.Items, pallet => Assert.NotEqual(Guid.Empty, pallet.PalletId));
			Assert.Contains(
				result.Items,
				pallet => pallet.PalletNumber == "Q1000" &&
					pallet.LocationId == 1 &&
					pallet.LocationName == "2-1-3-4");
			Assert.Contains(
				result.Items,
				pallet => pallet.PalletNumber == "Q2000" &&
					pallet.LocationId == 3 &&
					pallet.LocationName == "3-24-4-5");
		}

		[Fact]
		public async Task GetPalletToTakeOff_ShouldReturnEmptyPage_WhenIssueIsArchived()
		{
			// Arrange
			await AddArchivedIssueAsync(includePallet: true);

			// Act
			var result = await _issueReadService.GetPalletToTakeOff(
				ArchivedIssueId,
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		private Task<PagedResult<IssueSimplyDTO>> FindAsync(
			IssueReceiptSearchFilter filter)
			=> _issueReadService.GetIssueByFilter(
				filter,
				pageNumber: 1,
				pageSize: 20,
				CancellationToken.None);

		private async Task AddArchivedIssueAsync(bool includePallet = false)
		{
			DbContext.Issues.Add(CreateArchivedIssue());
			if (includePallet)
			{
				DbContext.Pallets.Add(Pallet.CreateForSeed(
					ArchivedPalletId,
					"Q0000",
					new DateTime(2025, 1, 1),
					locationId: 1,
					PalletStatus.ToIssue,
					receiptId: null,
					issueId: ArchivedIssueId));
			}

			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();
		}

		private static Issue CreateArchivedIssue()
			=> CreateIssue(ArchivedIssueId, issueNumber: 1, IssueStatus.Archived);

		private static Issue CreateIssue(
			Guid id,
			int issueNumber,
			IssueStatus status)
			=> Issue.CreateForSeed(
				id,
				issueNumber,
				clientId: 11,
				TestDates.UtcNow.AddDays(-1),
				TestDates.Today.AddDays(2),
				"U002",
				status,
				issueItems: []);
	}
}
