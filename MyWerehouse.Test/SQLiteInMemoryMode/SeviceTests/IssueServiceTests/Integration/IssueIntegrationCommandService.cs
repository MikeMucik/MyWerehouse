using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueIntegrationCommandService : TestBase
	{		
		protected readonly IssueService _issueService;
		protected readonly IMapper _mapper;

		protected readonly IIssueRepo _issueRepo;
		protected readonly IIssueItemRepo _issueItemRepo;

		protected readonly IPalletService _palletService;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IProductRepo _productRepo;
		protected readonly IAllocationRepo _allocationRepo;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IInventoryService _inventoryService;

		protected readonly ILocationService _locationService;
		protected readonly ILocationRepo _locationRepo;

		protected readonly IPalletMovementRepo _palletMovementRepo;	

		protected readonly IValidator<IssueItemDTO> _createItemValidator;
		protected readonly IValidator<CreateIssueDTO> _createIssueValidator;
		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;
		protected readonly IValidator<UpdateIssueDTO> _updateIssueValidator;

		protected readonly IEventCollector _eventCollector;
		
		public IssueIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();			
			_createItemValidator = new IssueItemDTOValidion();
			_createIssueValidator = new CreateIssueDTOValidion(_createItemValidator);
			_palletRepo = new PalletRepo(DbContext);
			_locationRepo = new LocationRepo(DbContext);
			_locationService = new LocationService(_locationRepo, _mapper, _palletRepo, DbContext);

			_palletMovementRepo = new PalletMovementRepo(DbContext);
			_issueRepo = new IssueRepo(DbContext);
			
			_productRepo = new ProductRepo(DbContext);
			_allocationRepo = new AllocationRepo(DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_updateIssueValidator = new UpdateIssueDTOValidation(_createItemValidator);
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
			_issueItemRepo = new IssueItemRepo(DbContext);
			_inventoryRepo = new InventoryRepo(DbContext);
			_inventoryService = new InventoryService(_inventoryRepo, _mapper, DbContext);
			_issueService = new IssueService(
				Mediator,
				_issueRepo,
				_mapper,
				DbContext,			
				_inventoryService,
				_palletRepo,
				_productRepo,
				_allocationRepo,
				_pickingPalletRepo,
				_palletService,
				_issueItemRepo,
				_createIssueValidator
				, _eventCollector
				);
		}
	}
}
