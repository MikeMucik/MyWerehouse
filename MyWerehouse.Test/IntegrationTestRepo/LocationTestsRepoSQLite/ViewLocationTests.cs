using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;


namespace MyWerehouse.Test.UnitTestRepo.LocationTestsRepo
{
	[Collection("QueryCollection")]
	public class ViewLocationTests
	{
		private readonly LocationRepo _locationRepo;
		private readonly QueryTestFixture _fixture;
		public ViewLocationTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_locationRepo = new LocationRepo(_fixture.DbContext);
		}

		[Fact]
		public async Task GetLocation_GetLocationByIdAsync_ReturnLocation()
		{
			//Arrange
			var locationId = 1;
			//Act
			var result = await _locationRepo.GetLocationByIdAsync(locationId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(locationId, result.Id);
			Assert.Equal(1, result.Aisle);
			Assert.Equal(2, result.Bay);
			Assert.Equal(3, result.Position);
			Assert.Equal(4, result.Height);
		}
		[Fact]
		public void GetEmptyLocation_GetAllAvailableLocations_ReturnList()
		{
			//Arrange&Act			
			var result = _locationRepo.GetAllAvailableLocations();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			Assert.Equal(2, result.First().Id);
			Assert.Equal(20, result.Last().Id);
		}
		[Fact]
		public async Task ReturnLocation_FindLocationAsync_ResultOk()
		{
			//Arrange
			int bay = 2;
			int aisle = 1;
			int position = 3;
			int height = 4;
			//Act
			var result = await _locationRepo.FindLocationAsync(bay, aisle, position, height);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
		}
	}
}
