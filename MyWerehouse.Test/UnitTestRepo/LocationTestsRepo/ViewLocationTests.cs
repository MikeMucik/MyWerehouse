using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.LocationTestsRepo
{	
	[Collection("QuerryCollection")]
	public class ViewLocationTests :CommandTestBase		
	{
		private readonly LocationRepo _locationRepo;
		public ViewLocationTests(QuerryTestFixture fixture)
		{
			var context = fixture.Context;
			_locationRepo = new LocationRepo(context);
		}
		[Fact]
		public void GetLocation_GetLocationById_ReturnLocation()
		{
			//Arrange
			var locationId = 1;
			//Act
			var result = _locationRepo.GetLocationById(locationId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(locationId, result.Id);
			Assert.Equal(1, result.Aisle);
			Assert.Equal(2, result.Bay);
			Assert.Equal(3, result.Position);
			Assert.Equal(4, result.Height);
		}
		[Fact]
		public async Task GetLocation_GetLocationByIdAsync_ReturnLocation()
		{
			//Arrange
			var locationId = 1;
			//Act
			var result =await _locationRepo.GetLocationByIdAsync(locationId);
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
			Assert.Equal(20,result.Last().Id);
		}
	}
}
