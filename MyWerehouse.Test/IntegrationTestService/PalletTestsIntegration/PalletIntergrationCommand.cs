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
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.PalletTestsIntegration
{
	public class PalletIntergrationCommand : CommandTestBase
	{
		protected readonly PalletService _palletService;
		protected readonly IMapper _mapper;

		protected readonly IPalletRepo _palletRepo;

		protected readonly ILocationService _locationService;
		protected readonly ILocationRepo _locationRepo;

		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IHistoryIssueRepo _historyIssueRepo;
		protected readonly IHistoryPickingRepo _historyAllocationRepo;
		protected readonly IHistoryReceiptRepo _historyReceiptRepo;

		protected readonly IHistoryService _historyService;



		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;

		public PalletIntergrationCommand() : base()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();

			_palletRepo = new PalletRepo(_context);

			_locationRepo = new LocationRepo(_context);
			_locationService = new LocationService(_locationRepo, _mapper, _context);

			_palletMovementRepo = new PalletMovementRepo(_context);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_historyIssueRepo = new HistoryIssueRepo(_context);
			_historyReceiptRepo = new HistoryReceiptRepo(_context);
			_historyAllocationRepo = new HistoryPickingRepo(_context);
			_historyService = new HistoryService(_palletMovementRepo, _historyIssueRepo, _historyReceiptRepo, _historyAllocationRepo);
			_pickingPalletRepo = new PickingPalletRepo(_context);
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_palletService = new PalletService(_palletRepo,
				_historyService,
				_locationService,
				_palletMovementRepo,				
				_pickingPalletRepo,
				_mapper,
				_updatePalletValidator,
				_context);
		}
	}
}
