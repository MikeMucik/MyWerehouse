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
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptIntegratioCommandService : TestBase
	{
		protected readonly ReceiptService _receiptService;
		protected readonly IMapper _mapper;
		protected readonly IReceiptRepo _receiptRepo;
		protected readonly IPalletService _palletService;
		protected readonly IPalletRepo _palletRepo;				

		protected readonly IValidator<ProductOnPalletDTO> _productOnPalletValidator;
		protected readonly IValidator<CreatePalletReceiptDTO> _palletValidator;
		protected readonly IValidator<ReceiptDTO> _receiptValidator;
		protected readonly IValidator<UpdatePalletDTO> _updateValidator;
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IInventoryService _inventoryService;
		public ReceiptIntegratioCommandService()
		{
			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();

			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_updateValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_receiptValidator = new ReceiptDTOValidation(_updateValidator);
			_palletValidator = new CreatePalletReceiptDTOValidation(_productOnPalletValidator);
			_palletRepo = new PalletRepo(DbContext);
			
			_receiptRepo = new ReceiptRepo(DbContext);
			_palletRepo = new PalletRepo(DbContext);
			_inventoryRepo = new InventoryRepo(DbContext);
			_inventoryService = new InventoryService(_inventoryRepo, _mapper, DbContext);
			_receiptService = new ReceiptService(Mediator,
				_receiptRepo,
				_mapper,
				DbContext,
				_palletRepo,				
				_inventoryService,
				_palletValidator,
				_receiptValidator);
		}
	}
}
