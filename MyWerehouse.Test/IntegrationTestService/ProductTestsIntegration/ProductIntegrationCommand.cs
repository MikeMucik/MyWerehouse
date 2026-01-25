using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	public class ProductIntegrationCommand : CommandTestBase
	{
		protected readonly ProductService _productService;
		protected readonly IValidator<AddProductDTO> _productValidator;
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IProductRepo _productRepo;
		protected readonly IReceiptRepo _receiptRepo;
		public ProductIntegrationCommand() : base()
		{	
			_productRepo = new ProductRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_productValidator = new AddProductDTOValidation();	
			_inventoryRepo = new InventoryRepo(_context);
			_productService = new ProductService(_productRepo, _mapper,_context,_inventoryRepo, _receiptRepo, _productValidator);
		}
	}
}
