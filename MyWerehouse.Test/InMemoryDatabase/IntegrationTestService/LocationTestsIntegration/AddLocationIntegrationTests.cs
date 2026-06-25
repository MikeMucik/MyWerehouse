using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.LocationTestsIntegration
{
	public class AddLocationIntegrationTests : LocationIntegrationCommand
	{
		[Fact]
		public async Task AddLocation_ShouldAddNewPlaceForPallet_WhenOneItemPrepared()
		{
			//Arrange
			var locationDTO = new LocationDTO
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1,
			};
			//Act
			var result = await _locationService.AddLocationServiceAsync(locationDTO);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Equal(1, result.Result);
			Assert.Contains("Dodano lokalizacje.", result.Message);
		}
		[Fact]
		public async Task AddLocation_ShouldReturnErrorInfo_WhenLocationExist()
		{
			//Arrange
			var locationDTO = new LocationDTO
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1,
			};
			//Act 1
			var result1 = await _locationService.AddLocationServiceAsync(locationDTO);
			//Assert 1
			Assert.NotNull(result1);
			Assert.True(result1.IsSuccess);
			Assert.Equal(1, result1.Result);
			//Act 2
			var result2 = await _locationService.AddLocationServiceAsync(locationDTO);
			Assert.NotNull(result2);
			Assert.False(result2.IsSuccess);
			Assert.Contains("Lokalizacja o zadanych parametrach już istnieje.", result2.Error);
		}
		[Fact]
		public async Task PrepareAndAddManyLocation_ShouldAddLocationsToBase_WhenListCorrect()
		{
			//Arrange
			var bay = 2;
			var aisleStart = 1;
			var aisleEnd = 3;
			var position = 4;
			var height = 5;
			//Act 1
			var result =_locationService.PrepareLocations(bay, aisleStart,aisleEnd, position, height);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.IsType<List<LocationDTO>>(result.Result);
			//Act 2
			var resultAdding = await _locationService.CreateManyLocation(result.Result);
			//Assert
			Assert.NotNull(resultAdding);
			Assert.True(resultAdding.IsSuccess);
			Assert.Contains("Dodano zbiór lokalizacji.", resultAdding.Message);
		}
		[Fact]
		public async Task PrepareAndAddManyLocation_ReturnError_WhenLocationExist()
		{
			//Arrange
			var locationDTO = new LocationDTO
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1,
			};
			//Act 1
			var result1 = await _locationService.AddLocationServiceAsync(locationDTO);
			//Assert 1
			Assert.NotNull(result1);
			Assert.True(result1.IsSuccess);
			Assert.Equal(1, result1.Result);
			//Arrange
			var bay = 1;
			var aisleStart = 1;
			var aisleEnd = 3;
			var position = 4;
			var height = 5;
			//Act 1
			var result = _locationService.PrepareLocations(bay, aisleStart, aisleEnd, position, height);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.IsType<List<LocationDTO>>(result.Result);
			//Act 2
			var resultAdding = await _locationService.CreateManyLocation(result.Result);
			//Assert
			Assert.NotNull(resultAdding);
			Assert.False(resultAdding.IsSuccess);
			Assert.Contains($"Lokalizacja o parametrach Bay = 1, Aisle = 1, Position = 1, Height = 1 już istnieje.", resultAdding.Error);
		}
	}
}
