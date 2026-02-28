using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using System.Reflection;
using MyWerehouse.Application.Common.Mapping;

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
			var details = new ProductDetail
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
		//[Fact]
		//public void Should_Map_AddPalletDTO_To_Pallet()
		//{

		//	var dto = new AddPalletDTO
		//	{
		//		Id = "Q0001",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		ReceiptId = 1,
		//		Status = PalletStatus.Available,

		////		ProductsOnPallet = new List<ProductOnPalletDTO>
		////{
		////	new ProductOnPalletDTO
		////	{
		////		Id = 1,
		////		ProductId = 10,
		////		PalletId = "Q0001",
		////		Quantity = 100,
		////		DateAdded = DateTime.Now,
		////		BestBefore = new DateOnly(2025, 1, 1)
		////	}
		////}
		//	};

		//	var entity = _mapper.Map<Pallet>(dto);

		//	Assert.NotNull(entity);
		//	Assert.Equal(dto.Id, entity.Id);
		//	//Assert.Single(entity.ProductsOnPallet);
		//	//Assert.Equal(10, entity.ProductsOnPallet.First().ProductId);
		//}
		[Fact]
		public void Should_Map_AddPalletReceiptDTO_To_Pallet()
		{
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var dto = new CreatePalletReceiptDTO
			{
				Id = "Q0001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				ReceiptId = receiptId1,
				ReceiptNumber = 1,
				Status = PalletStatus.Available,
			};

			var entity = _mapper.Map<Pallet>(dto);

			Assert.NotNull(entity);
			Assert.Equal(dto.Id, entity.Id);
		}
		//[Fact]
		//public void Should_Map_AddPalletPickingDTO_To_Pallet()
		//{
		//	var dto = new CreatePalletPickingDTO
		//	{
		//		Id = "Q0001",
		//		DateCreated = DateTime.Now,
		//		LocationId = 1,
		//		IssueId = 1,
		//		Status = PalletStatus.ToIssue,
		//	};
		//	var entity = _mapper.Map<Pallet>(dto);

		//	Assert.NotNull(entity);
		//	Assert.Equal(dto.Id, entity.Id);
		//}
		//[Fact]
		//public void Should_Map_Single_ProductOnPalletDTO_To_ProductOnPallet()
		//{
		//	var dto = new ProductOnPalletDTO
		//	{
		//		Id = 1,
		//		ProductId = 10,
		//		PalletId = "Q0001",
		//		Quantity = 100,
		//		DateAdded = DateTime.Now,
		//		BestBefore = new DateOnly(2025, 1, 1)
		//	};

		//	var entity = _mapper.Map<ProductOnPallet>(dto);

		//	Assert.NotNull(entity);
		//	Assert.Equal(dto.ProductId, entity.ProductId);
		//	Assert.Equal(dto.Quantity, entity.Quantity);
		//}

	}
}
