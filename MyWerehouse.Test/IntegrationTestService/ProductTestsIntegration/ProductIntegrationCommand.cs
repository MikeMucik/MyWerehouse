using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Mapping;
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
		public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly ProductService _productService;
		public readonly IMapper _mapper;
		public readonly IValidator<AddProductDTO> _productValidator;
		public readonly IProductRepo _productRepo;
		public readonly IReceiptRepo _receiptRepo;
		public ProductIntegrationCommand() : base()
		{
			//_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
			//	.UseInMemoryDatabase("SharedTestDatabase")
			//	.Options;

			var MapperConfig = new MapperConfiguration(cfg =>
			{ 
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();
			_productRepo = new ProductRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_productValidator = new AddProductDTOValidation();				
			_productService = new ProductService(_productRepo, _mapper,_context, _receiptRepo, _productValidator);
		}
	}
}
