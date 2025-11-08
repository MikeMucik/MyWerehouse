using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using static MyWerehouse.Application.ViewModels.IssueModels.CreateIssueDTO;
using static MyWerehouse.Application.ViewModels.IssueModels.UpdateIssueDTO;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class PalletIntegrationCommandService : TestBase
	{
		protected readonly PalletService _palletService;
		protected readonly IMapper _mapper;
		protected readonly IPalletRepo _palletRepo;

		protected readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly ILocationRepo _locationRepo;
		protected readonly ILocationService _locationService;

		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;
		protected readonly IEventCollector _eventCollector;
		public PalletIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();

			_palletRepo = new PalletRepo(DbContext);
			_palletMovementRepo = new PalletMovementRepo(DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			_locationRepo = new LocationRepo(DbContext);
			_locationService = new LocationService(_locationRepo, _mapper, _palletRepo, DbContext);

			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_eventCollector = new EventCollector();
			_palletService = new PalletService(Mediator,
				_palletRepo,
				_locationService,
				_palletMovementRepo,
				_pickingPalletRepo,
				_locationRepo,
				_mapper,
				_updatePalletValidator
				, DbContext
				,_eventCollector);
		}
	}
}
