using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ProductTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ProductIntegrationView : CommandTestBase
	{
		public readonly ProductService _productService;
		public readonly ProductRepo _productRepo;
		public ProductIntegrationView(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_productRepo = new ProductRepo(_context);			
			_productService = new ProductService(_productRepo, _mapper);
		}
	}

}
