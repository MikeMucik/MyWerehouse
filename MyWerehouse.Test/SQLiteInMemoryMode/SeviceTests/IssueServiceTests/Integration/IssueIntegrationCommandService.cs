using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
		protected readonly IPalletService _palletService;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IProductRepo _productRepo;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IInventoryService _inventoryService;

		protected readonly ILocationService _locationService;
		protected readonly ILocationRepo _locationRepo;

		protected readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IHistoryIssueRepo _historyIssueRepo;
		protected readonly IHistoryService _historyService;
		protected readonly IHistoryPickingRepo _historyAllocationRepo;
		protected readonly IHistoryReceiptRepo _historyReceiptRepo;

		protected readonly IValidator<IssueItemDTO> _createItemValidator;
		protected readonly IValidator<CreateIssueDTO> _createIssueValidator;
		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;
		protected readonly IValidator<UpdateIssueDTO> _updateIssueValidator;

		protected readonly IIssueItemRepo _issueItemRepo;
		public IssueIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();

			_createItemValidator = new IssueItemDTOValidion();
			_createIssueValidator = new CreateIssueDTOValidion(_createItemValidator);

			_locationRepo = new LocationRepo(DbContext);
			_locationService = new LocationService(_locationRepo, _mapper, DbContext);

			_palletMovementRepo = new PalletMovementRepo(DbContext);
			_historyIssueRepo = new HistoryIssueRepo(DbContext);
			_historyAllocationRepo = new HistoryPickingRepo(DbContext);
			_historyReceiptRepo = new HistoryReceiptRepo(DbContext);
			_historyService = new HistoryService(_palletMovementRepo, _historyIssueRepo, _historyReceiptRepo, _historyAllocationRepo);
			_issueRepo = new IssueRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_productRepo = new ProductRepo(DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			//_updateIssueValidator = new UpdateIssueDTOValidion();
			_palletService = new PalletService(
				_palletRepo,
				_historyService,
				_locationService,
				_palletMovementRepo,
				_pickingPalletRepo,
				_mapper,
				_updatePalletValidator
				, DbContext);
			_issueItemRepo = new IssueItemRepo(DbContext);
			_inventoryRepo = new InventoryRepo(DbContext);
			_inventoryService = new InventoryService(_inventoryRepo, _mapper);
			_issueService = new IssueService(
				_issueRepo,
				_mapper,
				DbContext,
				_historyService,
				_inventoryService,
				_palletRepo,
				_productRepo,
				_pickingPalletRepo,
				_palletService,
				_issueItemRepo,
				_createIssueValidator,
				_updateIssueValidator
				);
		}
	}
}
