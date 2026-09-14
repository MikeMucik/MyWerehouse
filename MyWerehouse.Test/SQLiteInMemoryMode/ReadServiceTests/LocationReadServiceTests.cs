using MyWerehouse.Infrastructure.Persistence.ReadServices;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class LocationReadServiceTests : TestBase
	{
		private readonly LocationReadService _locationReadService;

		public LocationReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_locationReadService = new LocationReadService(DbContext);
		}

		[Fact]
		public async Task GetLocationByIdAsync_ShouldReturnLocationDTO_WhenLocationExists()
		{
			// Act
			var result = await _locationReadService.GetLocationByIdAsync(
				1,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
			Assert.Equal(2, result.Bay);
			Assert.Equal(1, result.Aisle);
			Assert.Equal(3, result.Position);
			Assert.Equal(4, result.Height);
		}

		[Fact]
		public async Task GetLocationByIdAsync_ShouldReturnNull_WhenLocationDoesNotExist()
		{
			// Act
			var result = await _locationReadService.GetLocationByIdAsync(
				999,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task FindLocationIdAsync_ShouldReturnLocationId_WhenCoordinatesMatch()
		{
			// Act
			var result = await _locationReadService.FindLocationIdAsync(
				bay: 2,
				aisle: 1,
				position: 3,
				height: 4,
				CancellationToken.None);

			// Assert
			Assert.Equal(1, result);
		}

		[Fact]
		public async Task FindLocationIdAsync_ShouldReturnNull_WhenCoordinatesDoNotMatch()
		{
			// Act
			var result = await _locationReadService.FindLocationIdAsync(
				bay: 999,
				aisle: 999,
				position: 999,
				height: 999,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}
	}
}
