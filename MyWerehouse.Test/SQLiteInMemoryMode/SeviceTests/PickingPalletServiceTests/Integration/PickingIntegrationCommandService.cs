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
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class PickingIntegrationCommandService : TestBase
	{
		protected readonly PickingPalletService _pickingPalletService;
		protected readonly IMapper _mapper;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly ILocationRepo _locationRepo;
		protected readonly IPalletRepo _palletRepo;
		protected readonly IIssueRepo _issueRepo;
		protected readonly IPalletService _palletService;
		protected readonly IHistoryService _historyService;

		protected readonly ILocationService _locationService;
		protected readonly IPalletMovementRepo _movementRepo;
		protected readonly IValidator<UpdatePalletDTO> _updatevalidator;

		protected readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IHistoryIssueRepo _historyIssueRepo;
		protected readonly IHistoryReceiptRepo _historyReceiptRepo;
		protected readonly IHistoryPickingRepo _historyPickingRepo;

		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		public PickingIntegrationCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();
			_locationRepo = new LocationRepo(DbContext);			
			_palletMovementRepo = new PalletMovementRepo(DbContext);			
			_locationService = new LocationService(_locationRepo, _mapper, DbContext);
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updatevalidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_palletMovementRepo = new PalletMovementRepo(DbContext);
			_historyIssueRepo = new HistoryIssueRepo(DbContext);
			_historyPickingRepo = new HistoryPickingRepo(DbContext);
			_historyReceiptRepo = new HistoryReceiptRepo(DbContext);

			_historyService = new HistoryService(_palletMovementRepo, _historyIssueRepo, _historyReceiptRepo, _historyPickingRepo);
			_issueRepo = new IssueRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_palletService = new PalletService(_palletRepo, _historyService, _locationService, _palletMovementRepo, _pickingPalletRepo, _mapper, _updatevalidator, DbContext);
			
			_pickingPalletRepo = new PickingPalletRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_pickingPalletService = new PickingPalletService(_pickingPalletRepo, DbContext, _locationRepo,_palletRepo, _issueRepo, _historyService,_palletService);
		}
	}
}
