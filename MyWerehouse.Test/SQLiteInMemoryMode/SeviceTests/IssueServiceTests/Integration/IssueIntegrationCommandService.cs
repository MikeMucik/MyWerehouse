using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using static MyWerehouse.Application.ViewModels.IssueModels.CreateIssueDTO;

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
		protected readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly IHistoryIssueRepo _historyIssueRepo;
		protected readonly IPalletMovementService _palletMovementService;	
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IInventoryService _inventoryService;
		protected readonly IValidator<IssueItemDTO> _createItemValidator;
		protected readonly IValidator<CreateIssueDTO> _createIssueValidator;
		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;
		public IssueIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();

			_createItemValidator = new IssueItemDTOValidion();
			_createIssueValidator = new CreateIssueDTOValidion(_createItemValidator);	
			
			_palletMovementRepo = new PalletMovementRepo(DbContext);
			_historyIssueRepo = new HistoryIssueRepo(DbContext);
			_palletMovementService = new PalletMovementService(_palletMovementRepo, _historyIssueRepo);
			_issueRepo = new IssueRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_productRepo = new ProductRepo(DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_palletService = new PalletService(
				_palletRepo,
				_palletMovementService,
				_palletMovementRepo,
				_pickingPalletRepo,
				_mapper,
				_updatePalletValidator
				,DbContext);
			_inventoryRepo = new InventoryRepo(DbContext);			
			_issueService = new IssueService(
				_issueRepo,
				_mapper,
				DbContext,
				_palletMovementService,
				_inventoryRepo,
				_palletRepo,
				_productRepo,
				_pickingPalletRepo,
				_palletService,				
				_createIssueValidator
				);
		}
	}
}
