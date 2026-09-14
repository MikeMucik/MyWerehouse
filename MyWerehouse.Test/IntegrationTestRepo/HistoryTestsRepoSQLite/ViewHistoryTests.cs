using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.IntegrationTestRepo.HistoryTestsRepoSQLite
{
	[Collection("QueryCollectionInMemory")]
	public class ViewHistoryTests
	{
		private readonly HistoryPalletRepo _palletMovementRepo;
		public ViewHistoryTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_palletMovementRepo = new HistoryPalletRepo(_context);
		}
		[Fact]
		public async Task CanDeletePalletAsync_ReturnFalse_IsCanDelete()
		{
			//Arrange
			var palletId =Guid.Parse("00000000-0001-1111-0000-000000000000");
			//Act
			var result =await _palletMovementRepo.CanDeletePalletAsync(palletId, CancellationToken.None);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task IsCanDelete_CanDeletePalletAsync_ReturnTrue()
		{
			//Arrange
			var palletId = Guid.Parse("00000000-0002-1111-0000-000000000000");
			//Act
			var result = await _palletMovementRepo.CanDeletePalletAsync(palletId, CancellationToken.None);
			//Assert
			Assert.True(result);
		}
	}
}
