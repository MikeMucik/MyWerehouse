using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class PickingIntegrationCommandService : TestBase
	{
		protected readonly PickingPalletService _pickingPalletService;
		//protected readonly IMediator _mediator;	
		protected readonly IMapper _mapper;
		protected readonly IAllocationRepo _allocationRepo;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly ILocationRepo _locationRepo;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IIssueRepo _issueRepo;
		protected readonly IPalletService _palletService;
	
		protected readonly ILocationService _locationService;
		protected readonly IPalletMovementRepo _movementRepo;
		protected readonly IValidator<UpdatePalletDTO> _updatevalidator;

		protected readonly IPalletMovementRepo _palletMovementRepo;		

		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;

		protected readonly IEventCollector _eventCollector;
		
		public PickingIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();			
			_palletRepo = new PalletRepo(DbContext);
			_locationRepo = new LocationRepo(DbContext);			
			_palletMovementRepo = new PalletMovementRepo(DbContext);			
			_locationService = new LocationService(_locationRepo, _mapper, _palletRepo, DbContext);
			_allocationRepo = new AllocationRepo(DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updatevalidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_palletMovementRepo = new PalletMovementRepo(DbContext);
			_issueRepo = new IssueRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_eventCollector = new EventCollector();
			_palletService = new PalletService(Mediator, _palletRepo				
				, _locationService, _palletMovementRepo
				, _pickingPalletRepo, _locationRepo, _mapper, _updatevalidator, DbContext
				,_eventCollector);
			
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			
			_pickingPalletService = new PickingPalletService(Mediator, _pickingPalletRepo, _allocationRepo, DbContext, _locationRepo,_palletRepo, _issueRepo, _palletService, _eventCollector);
		}
	}
}
