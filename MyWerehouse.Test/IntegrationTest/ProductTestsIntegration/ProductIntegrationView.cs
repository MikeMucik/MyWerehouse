using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.ProductTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ProductIntegrationView : CommandTestBase
	{
		public readonly ProductService _productService;
		public readonly ProductRepo _productRepo;
		public readonly IMapper _mapper;
		public ProductIntegrationView(QuerryTestFixture fixture)
		{
			var context = fixture.Context;
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = mapperConfig.CreateMapper();
			_productRepo = new ProductRepo(context);
			_productService = new ProductService(_productRepo, _mapper);
		}
	}
}
