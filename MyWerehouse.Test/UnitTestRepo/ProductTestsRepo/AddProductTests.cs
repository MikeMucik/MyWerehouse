using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductTestsRepo
{
	public class AddProductTests : CommandTestBase
	{
		private readonly ProductRepo _productRepo;
		public AddProductTests() : base()
		{
			_productRepo = new ProductRepo(_context);
		}
		[Fact]
		public void AddProperData_AddProduct_ShouldAddToCollection()
		{
			//Arrange
			var productRepo = new Product
			{
				Id = 1,
				Name = "Banana",
				SKU = "1234567890",
				CategoryId = 1,
			};
			//Act
			var result = _productRepo.AddProduct(productRepo);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result);
		}
	}
}
