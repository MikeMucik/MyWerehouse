using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	public class ProductIntegrationCommand : CommandTestBase
	{
		public readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public readonly ProductService _productService;
		public readonly IMapper _mapper;
		public ProductIntegrationCommand() : base()
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("SharedTestDatabase")
				.Options;

			var MapperConfig = new MapperConfiguration(cfg =>
			{ 
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = MapperConfig.CreateMapper();
			var _productRepo = new ProductRepo(_context);
			var _receiptRepo = new ReceiptRepo(_context);
			_productService = new ProductService(_productRepo, _mapper);
			_productService = new ProductService(_productRepo, _mapper, _receiptRepo);
		}
	}
}
