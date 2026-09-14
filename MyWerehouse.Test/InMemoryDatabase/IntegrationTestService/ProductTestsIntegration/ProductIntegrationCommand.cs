using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Server;
using MyWerehouse.Server.ServicesToInfrastructure;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ProductTestsIntegration
{
	public class ProductIntegrationCommand : CommandTestBase
	{
		protected readonly ProductService _productService;
		protected readonly IUnitOfWork _unitOfWork;
		protected readonly IProductReadService _productReadService;
		protected readonly IValidator<EditProductDTO> _productValidator;
		protected readonly IValidator<CreateProductDTO> _createProductValidator;
		protected readonly IInventoryRepo _inventoryRepo;
		protected readonly IProductRepo _productRepo;
		protected readonly IReceiptRepo _receiptRepo;
		protected readonly ICategoryRepo _categoryRepo;
		public ProductIntegrationCommand() : base()
		{	
			_productRepo = new ProductRepo(_context);
			_unitOfWork = new UnitOfWork(_context);
			_productReadService = new ProductReadService(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_productValidator = new EditProductDTOValidation();	
			_createProductValidator = new AddProductDTOValidation();
			_inventoryRepo = new InventoryRepo(_context);
			_categoryRepo = new CategoryRepo(_context);
			_productService = new ProductService(_productRepo,_unitOfWork, _productReadService, _inventoryRepo,_categoryRepo, _receiptRepo,_createProductValidator, _productValidator, new TestDateTimeProvider());
		}
	}
}
