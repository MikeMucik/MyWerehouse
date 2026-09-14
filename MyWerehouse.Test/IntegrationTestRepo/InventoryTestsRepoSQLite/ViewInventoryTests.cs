using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.InventoryTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewInventoryTests: TestBase
	{
		public readonly InventoryRepo _inventoryRepo;
		private readonly QueryTestSQLFixture _fixture;
		public ViewInventoryTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_inventoryRepo = new InventoryRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task CheckStockEnough_HasStockAsync_ReturnTrue()
		{
			//Arrange&Act
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var quantity = 8;
			var result = await _inventoryRepo.HasStockAsync(productId1, quantity, CancellationToken.None);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public async Task CheckStockNotEnough_HasStockAsync_ReturnFalse()
		{
			//Arrange&Act
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var quantity = 80;
			var result = await _inventoryRepo.HasStockAsync(productId1, quantity, CancellationToken.None);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task GetAllocatableQuantityAsync_IgnoresPalletContainingDifferentProducts()
		{
			//Arrange
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");
			var bestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(30));
			//var
			//Act
			var result = await _inventoryRepo.GetAllocatableQuantityAsync(productId2, bestBefore, CancellationToken.None);
			//Assert
			Assert.Equal(450, result);
		}
	}
}
