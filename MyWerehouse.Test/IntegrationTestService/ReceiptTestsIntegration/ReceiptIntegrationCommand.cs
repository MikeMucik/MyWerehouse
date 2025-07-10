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
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ReceiptTestsIntegration
{
	public class ReceiptIntegrationCommand :CommandTestBase
	{
		public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly ReceiptService _receiptService;
		public readonly IMapper _mapper;
		public readonly IReceiptRepo _receiptRepo;
		public readonly IPalletService _palletService;
		public readonly IPalletRepo _palletRepo;
		public readonly IProductOnPalletRepo _productOnPalletRepo;
		public readonly IPalletMovementService _palletMovementService;
		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;		
		protected readonly IValidator<CreatePalletReceiptDTO> _validator;
		protected readonly IValidator<ReceiptDTO> _receiptValidator;
		protected readonly IValidator<UpdatePalletDTO> _validatorUpdate;



		public ReceiptIntegrationCommand() : base() 
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();
			_receiptRepo = new ReceiptRepo(_context);

			_palletRepo = new PalletRepo(_context);
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
			var _palletMovementRepo = new PalletMovementRepo(_context);
			_palletMovementService = new PalletMovementService(_palletMovementRepo);

			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_validator = new CreatePalletReceiptDTOValidation(_productOnPalletValidator);
			_validatorUpdate = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_receiptValidator = new ReceiptDTOValidation(_validatorUpdate);
			
			_receiptService =new ReceiptService(_receiptRepo, _mapper,_context,_palletRepo, _palletMovementService, _validator, _receiptValidator, _validatorUpdate);
		}
	}
}
