using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Infrastructure.Persistence.ReadServices;
using MyWerehouse.Test.InMemoryDatabase.Common;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.LocationTestsIntegration
{
	public class LocationIntegrationCommand : CommandTestBase
	{		
		protected readonly LocationService _locationService;
		protected readonly ILocationRepo _locationRepo;
		protected readonly ILocationReadService _locationReadService;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IUnitOfWork _unitOfWork;

		public LocationIntegrationCommand() : base()
		{
			_locationRepo = new LocationRepo(_context);
			_locationReadService = new LocationReadService(_context);
			_palletRepo = new PalletRepo(_context);
			_unitOfWork = new UnitOfWork(_context);
			_locationService = new LocationService(_locationRepo,_locationReadService, _palletRepo, _unitOfWork);
		}
	}
}
