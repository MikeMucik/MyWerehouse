using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure.Persistence.ReadServices;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class HistoryReadServiceTests : TestBase
	{
		private static readonly Guid PalletId =
			Guid.Parse("00000000-0001-1111-0000-000000000000");
		private static readonly Guid PalletWithoutRelationsId =
			Guid.Parse("00000000-0009-1111-0000-000000000000");
		private static readonly Guid ProductId1 =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ProductId2 =
			Guid.Parse("00000000-0000-0000-0002-000000000000");
		private static readonly Guid ReceiptId =
			Guid.Parse("11111111-1111-1111-1111-111111111111");
		private static readonly Guid IssueId =
			Guid.Parse("11111111-2111-1111-1111-111111111111");

		private readonly HistoryReadService _historyReadService;

		public HistoryReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_historyReadService = new HistoryReadService(DbContext);
		}

		[Fact]
		public async Task GetHistoryPallet_ShouldReturnHeaderAndPagedMovements_WhenPalletExists()
		{
			// Arrange
			var firstMovement = DbContext.HistoryPallet.Single(history => history.Id == 1);
			firstMovement.SourceLocationSnapShot = "1-1-1-1";
			firstMovement.DestinationLocationSnapShot = "2-2-2-2";
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var firstPageResult = await _historyReadService.GetHistoryPallet(
				"Q1000",
				pageNumber: 1,
				pageSize: 1,
				CancellationToken.None);
			var secondPageResult = await _historyReadService.GetHistoryPallet(
				"Q1000",
				pageNumber: 2,
				pageSize: 1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(firstPageResult);
			Assert.Equal(PalletId, firstPageResult.Id);
			Assert.Equal("Q1000", firstPageResult.PalletNumber);
			Assert.Equal(new DateTime(2020, 1, 1), firstPageResult.DateReceived);
			Assert.Equal(ReceiptId, firstPageResult.ReceiptId);
			Assert.Equal(1, firstPageResult.ReceiptNumber);
			Assert.Equal(IssueId, firstPageResult.IssueId);
			Assert.Equal(2, firstPageResult.IssueNumber);

			var firstPage = firstPageResult.PalletMovementsDTO;
			Assert.Equal(2, firstPage.TotalCount);
			Assert.Equal(1, firstPage.CurrentPage);
			Assert.Equal(1, firstPage.PageSize);
			Assert.True(firstPage.HasNext);
			Assert.False(firstPage.HasPrevious);
			var movement = Assert.Single(firstPage.Items);
			Assert.Equal(1, movement.Id);
			Assert.Equal("Q1000", movement.PalletNumber);
			Assert.Equal("1-1-1-1", movement.LocationSnapShotSource);
			Assert.Equal("2-2-2-2", movement.LocationSnapShotDestination);
			Assert.Equal(ReasonForPallet.Moved, movement.Reason);
			Assert.Equal("TestUser", movement.PerformedBy);
			Assert.Equal(new DateTime(2025, 2, 2), movement.MovementDate);
			Assert.Equal(2, movement.HistoryPalletDetailsDTO.Count);
			Assert.Equal(
				100,
				movement.HistoryPalletDetailsDTO
					.Single(detail => detail.ProductId == ProductId1)
					.QuantityChange);
			Assert.Equal(
				1,
				movement.HistoryPalletDetailsDTO
					.Single(detail => detail.ProductId == ProductId2)
					.QuantityChange);

			Assert.NotNull(secondPageResult);
			var secondPage = secondPageResult.PalletMovementsDTO;
			Assert.Equal(2, secondPage.TotalCount);
			Assert.Equal(2, secondPage.CurrentPage);
			Assert.False(secondPage.HasNext);
			Assert.True(secondPage.HasPrevious);
			var secondMovement = Assert.Single(secondPage.Items);
			Assert.Equal(5, secondMovement.Id);
			Assert.Single(secondMovement.HistoryPalletDetailsDTO);
		}

		[Fact]
		public async Task GetHistoryPallet_ShouldReturnEmptyMovementsAndNullRelations_WhenPalletHasNoHistory()
		{
			// Act
			var result = await _historyReadService.GetHistoryPallet(
				"Q5000",
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(PalletWithoutRelationsId, result.Id);
			Assert.Equal("Q5000", result.PalletNumber);
			Assert.Null(result.ReceiptId);
			Assert.Null(result.ReceiptNumber);
			Assert.Null(result.IssueId);
			Assert.Null(result.IssueNumber);
			Assert.Equal(0, result.PalletMovementsDTO.TotalCount);
			Assert.Empty(result.PalletMovementsDTO.Items);
		}

		[Fact]
		public async Task GetHistoryPallet_ShouldReturnNull_WhenPalletDoesNotExist()
		{
			// Act
			var result = await _historyReadService.GetHistoryPallet(
				"Q9999",
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}
	}
}
