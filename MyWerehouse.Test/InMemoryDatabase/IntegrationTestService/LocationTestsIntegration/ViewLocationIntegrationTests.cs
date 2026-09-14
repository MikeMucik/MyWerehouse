using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Server;
using MyWerehouse.Server.ServicesToInfrastructure;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.LocationTestsIntegration
{
	[Collection("QueryCollectionInMemory")]
	public class ViewLocationIntegrationTests :CommandTestBase
	{
		protected readonly LocationService _locationService;
		protected readonly ILocationRepo _locationRepo;
		protected readonly ILocationReadService _locationReadService;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IUnitOfWork _unitOfWork;

		public ViewLocationIntegrationTests() : base()
		{
			_locationRepo = new LocationRepo(_context);
			_locationReadService = new LocationReadService(_context);
			_palletRepo = new PalletRepo(_context);
			_unitOfWork = new UnitOfWork(_context);
			_locationService = new LocationService(_locationRepo,_locationReadService, _palletRepo, _unitOfWork);
		}
		public async Task FindLocation_ShouldReturnLocationForParameters()
		{
			//Arrange&Act
			var result = await _locationService.FindLocationIdAsync(1, 1, 1, 1, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.IsType<Location>(result);
		}
		public async Task GetLocation_ShouldReturnLocationDTOForId()
		{
			//Arrange&Act
			var result = await _locationService.GetLocationServiceAsync(1, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.IsType<LocationDTO>(result);
		}
	}
}
