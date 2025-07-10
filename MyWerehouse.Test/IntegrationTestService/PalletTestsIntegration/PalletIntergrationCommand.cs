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
		//public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly PalletService _palletService;
		public readonly IMapper _mapper;
		public readonly IPalletRepo _palletRepo;
		//public readonly IProductOnPalletRepo _productOnPalletRepo;
		public readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<CreatePalletPickingDTO> _createPalletPickingValidator;
		//protected readonly IValidator<CreatePalletReceiptDTO> _createPalletReceiptValidator;
		protected readonly IValidator<UpdatePalletDTO> _updatePalletValidator;
		//protected readonly WerehouseDbContext _werehouseDbContext;

		public PalletIntergrationCommand() : base()
		{			
			//_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
			//	.UseInMemoryDatabase("Shared Database")
			//	.Options;

			var MapperConfig = new MapperConfiguration(cfg =>
			{ 
				cfg.AddProfile<MappingProfile>();
			});
			_mapper  = MapperConfig.CreateMapper();

			_palletRepo = new PalletRepo(_context);
			//_productOnPalletRepo = new ProductOnPalletRepo(_context);
			_palletMovementRepo = new PalletMovementRepo(_context);

			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			//_createPalletReceiptValidator = new CreatePalletReceiptDTOValidation(_productOnPalletValidator);
			_createPalletPickingValidator = new CreatePalletPickingDTOValidation(_productOnPalletValidator);
			_updatePalletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			//_werehouseDbContext = new WerehouseDbContext(_contextOptions);
			_palletService = new PalletService(_palletRepo, _palletMovementRepo,
				_mapper, _createPalletPickingValidator, _updatePalletValidator,
				_context);
		}
	}
}
