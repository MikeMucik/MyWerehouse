using MyWerehouse.Infrastructure.Persistence.ReadServices;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class InventoryReadServiceTests : TestBase
	{
		private static readonly Guid ProductId1 =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ProductId2 =
			Guid.Parse("00000000-0000-0000-0002-000000000000");

		private readonly InventoryReadService _inventoryReadService;

		public InventoryReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_inventoryReadService = new InventoryReadService(DbContext);
		}

		[Fact]
		public async Task GetInventory_ShouldReturnProjection_WhenInventoryExists()
		{
			// Act
			var result = await _inventoryReadService.GetInventory(
				ProductId1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ProductId1, result.ProductId);
			Assert.Equal(10, result.Quantity);
			Assert.Equal(new DateTime(2025, 5, 6), result.LastUpdated);
		}

		[Fact]
		public async Task GetInventory_ShouldReturnNull_WhenInventoryDoesNotExist()
		{
			// Act
			var result = await _inventoryReadService.GetInventory(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetInventories_ShouldReturnOrderedPagedInventories()
		{
			// Act
			var firstPage = await _inventoryReadService.GetInventories(
				pageNumber: 1,
				pageSize: 1,
				CancellationToken.None);
			var secondPage = await _inventoryReadService.GetInventories(
				pageNumber: 2,
				pageSize: 1,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, firstPage.TotalCount);
			Assert.Equal(1, firstPage.CurrentPage);
			Assert.Equal(1, firstPage.PageSize);
			Assert.True(firstPage.HasNext);
			Assert.False(firstPage.HasPrevious);
			var firstInventory = Assert.Single(firstPage.Items);
			Assert.Equal(ProductId1, firstInventory.ProductId);
			Assert.Equal(10, firstInventory.Quantity);

			Assert.Equal(2, secondPage.TotalCount);
			Assert.Equal(2, secondPage.CurrentPage);
			Assert.False(secondPage.HasNext);
			Assert.True(secondPage.HasPrevious);
			var secondInventory = Assert.Single(secondPage.Items);
			Assert.Equal(ProductId2, secondInventory.ProductId);
			Assert.Equal(0, secondInventory.Quantity);
		}

		[Fact]
		public async Task GetInventories_ShouldReturnEmptyPage_WhenPageIsOutsideRange()
		{
			// Act
			var result = await _inventoryReadService.GetInventories(
				pageNumber: 3,
				pageSize: 1,
				CancellationToken.None);

			// Assert
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(3, result.CurrentPage);
			Assert.Equal(1, result.PageSize);
			Assert.Empty(result.Items);
			Assert.False(result.HasNext);
			Assert.True(result.HasPrevious);
		}
	}
}
