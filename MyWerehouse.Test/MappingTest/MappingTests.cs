using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;

namespace MyWerehouse.Test.MappingTest
{
	public class MappingTests
	{
		private readonly IMapper _mapper;	
		
		public MappingTests()
		{			
			var services = new ServiceCollection();
			services.AddLogging();
			
			services.AddAutoMapper(cfg =>
			{
				cfg.AddProfile<MappingProfile>();				
			});
			var serviceProvider = services.BuildServiceProvider();
			_mapper = serviceProvider.GetRequiredService<IMapper>();
		}
		[Fact]
		public void ShouldMap_AddProductDTO_To_Product()
		{
			//Arrange
			var productNew = new EditProductDTO
			{
				Name = "Apple",
				SKU = "666666",
				CategoryId = 1,
				Length = 100,
				Height = 200,
				Width = 300,
				Weight = 400,
				Description = "500",
			};
			//Act
			var product = _mapper.Map<Product>(productNew);
			//Assert
			Assert.NotNull(product);
			Assert.Equal(productNew.Name, product.Name);
			Assert.NotNull(product.Details);
			Assert.Equal(productNew.Length, product.Details.Length);
		}
		[Fact]
		public void ShouldMap_ToProductDetails_WhenDetailsOfProductDTO()
		{
			//Arrange
			var category = new Category { Id = 1, Name = "TestCategory" };
			
			var productNew = Product.Create("Apple", "666666", 1, 56);
			var details = ProductDetail.CreateDetails(productNew.Id, 100, 200, 120, 400, "500");
			
			productNew.SetDetails(details);
			productNew.SetCategory(category);
			//Act

			var product = _mapper.Map<DetailsOfProductDTO>(productNew);
			//Assert
			Assert.NotNull(product);
			Assert.Equal(product.Name, productNew.Name);
			Assert.NotNull(productNew.Details);
			Assert.Equal(product.Length, productNew.Details.Length);
			Assert.Equal(product.CategoryName, productNew.Category.Name); 
			Assert.Equal(product.CategoryId, productNew.CategoryId);
		}
	}
}
