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
		public readonly PalletService _palletService;
		public readonly IMapper _mapper;
		public readonly IPalletRepo _palletRepo;
		public readonly IPalletMovementRepo _palletMovementRepo;
		public readonly IPalletMovementService _palletMovementService;
		protected readonly IPickingPalletRepo _pickingPalletRepo;
		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		//protected readonly IValidator<CreatePalletPickingDTO> _createPalletPickingValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;

		public PalletIntergrationCommand() : base()
		{			
			var MapperConfig = new MapperConfiguration(cfg =>
			{ 
				cfg.AddProfile<MappingProfile>();
			});
			_mapper  = MapperConfig.CreateMapper();

			_palletRepo = new PalletRepo(_context);
			
			_palletMovementRepo = new PalletMovementRepo(_context);
//_createPalletPickingValidator = new CreatePalletPickingDTOValidation(_productOnPalletValidator);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			var historyRepo = new HistoryIssueRepo(_context);
			_palletMovementService = new PalletMovementService(_palletMovementRepo, historyRepo);
			_pickingPalletRepo = new PickingPalletRepo(_context);
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);			
			_palletService = new PalletService(_palletRepo,
				_palletMovementService,
				_palletMovementRepo,
				_pickingPalletRepo,
				_mapper,
				_updatePalletValidator,
				_context);
		}
	}
}
