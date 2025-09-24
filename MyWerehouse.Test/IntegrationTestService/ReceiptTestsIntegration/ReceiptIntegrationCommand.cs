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
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IInventoryService _inventoryService;

		public readonly IHistoryService _historyService;		
		protected readonly IPalletMovementRepo _palletMovementRepo;
		protected readonly IHistoryIssueRepo _historyIssueRepo;
		protected readonly IHistoryPickingRepo _historyAllocationRepo;
		protected readonly IHistoryReceiptRepo _historyReceiptRepo;

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
			_palletMovementRepo = new PalletMovementRepo(_context);
			_historyIssueRepo = new HistoryIssueRepo(_context);
			_historyAllocationRepo = new HistoryPickingRepo(_context);
			_historyReceiptRepo = new HistoryReceiptRepo(_context);
			_historyService = new HistoryService(_palletMovementRepo, _historyIssueRepo, _historyReceiptRepo, _historyAllocationRepo);
			_inventoryRepo = new InventoryRepo(_context);
			_inventoryService = new InventoryService(_inventoryRepo, _mapper);
			_productOnPalletValidator = new ProductOnPalletDTOValidation();
			_validator = new CreatePalletReceiptDTOValidation(_productOnPalletValidator);
			_validatorUpdate = new UpdatePalletDTOValidation(_productOnPalletValidator);
			_receiptValidator = new ReceiptDTOValidation(_validatorUpdate);
			
			_receiptService =new ReceiptService(_receiptRepo, _mapper,_context,_palletRepo, _historyService, _inventoryService, _validator, _receiptValidator);
		}
	}
}
