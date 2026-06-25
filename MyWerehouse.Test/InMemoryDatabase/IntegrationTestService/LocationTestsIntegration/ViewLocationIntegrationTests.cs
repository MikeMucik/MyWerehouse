using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.LocationTestsIntegration
{
	[Collection("QueryCollectionInMemory")]
	public class ViewLocationIntegrationTests :CommandTestBase
	{
		protected readonly LocationService _locationService;
		protected readonly ILocationRepo _locationRepo;
		protected readonly IPalletRepo _palletRepo;

		public ViewLocationIntegrationTests() : base()
			//(InMemoryDatabaseFixtureExecutive fixture) : base()
		{
			//var _context = InMemoryDatabaseFixtureExecutive.Context;
			_locationRepo = new LocationRepo(_context);
			_palletRepo = new PalletRepo(_context);
			_locationService = new LocationService(_locationRepo, _mapper, _palletRepo, _context);
		}
		public async Task FindLocation_ShouldReturnLocationForParameters()
		{
			//Arrange&Act
			var result = await _locationService.FindLocationAsync(1, 1, 1, 1);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.IsType<Location>(result);
		}

		public async Task GetLocation_ShouldReturnLocationDTOForId()
		{
			//Arrange&Act
			var result = await _locationService.GetLocationServiceAsync(1);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.IsType<LocationDTO>(result);
		}
	}
}
