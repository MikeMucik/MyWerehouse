using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.MappingTest
{
	public class MappingTests
	{
		private readonly IMapper _mapper;
		public MappingTests()
		{
			var mappingConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = mappingConfig.CreateMapper();
		}
		[Fact]
		public void ShouldMap_AddProductDTO_To_Product()
		{
			//Arrange
			var productNew = new AddProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
				AddedItemAd = DateTime.Now,
			};
			//Act
			var product = _mapper.Map<Product>(productNew);
			//Assert
			Assert.Equal(productNew.Name, product.Name);
			Assert.Equal(productNew.Length, product.Details.Length);
		}
		[Fact]
		public void ShouldMap_Product_To_DetailsOfProductDTO()
		{
			//Arrange
			var category = new Category { Id = 1, Name = "TestCategory" };
			var details = new ProductDetails
			{
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			var productNew = new Product
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Category = category,
				Details = details,
				AddedItemAd = DateTime.Now,
			};
			
			//Act
			var product = _mapper.Map<DetailsOfProductDTO>(productNew);
			//Assert
			Assert.Equal(product.Name, productNew.Name);
			
			Assert.Equal(product.Length, productNew.Details.Length);
			Assert.Equal(product.CategoryName, productNew.Category.Name);
		}
	}
}
