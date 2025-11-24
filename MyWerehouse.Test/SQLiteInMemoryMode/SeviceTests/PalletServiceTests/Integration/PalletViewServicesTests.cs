using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	[Collection("QueryCollection")]
	public class PalletViewServicesTests
	{
		private readonly PalletService _palletService;
		private readonly IMapper _mapper;
		private readonly PalletRepo _palletRepo;
		private readonly QueryTestFixture _fixture;

		public PalletViewServicesTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
				
			});
			_mapper = mapperConfig.CreateMapper();

			_palletRepo = new PalletRepo(_fixture.DbContext);
			_palletService = new PalletService(_palletRepo, _mapper);
		}
		[Fact]
		public async Task ShowDataToEdit_GetPalletToEditAsync_ReturnUpdatePalletDTO()
		{
			//Arrange
			var palletId = "Q1001";
			//Act
			var result = await _palletService.GetPalletToEditAsync(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.Single(result.ProductsOnPallet);
			Assert.Equal(1, result.LocationId);
			Assert.Equal(PalletStatus.OnHold, result.Status);
			Assert.Equal(1, result.ReceiptId);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			var product1 = result.ProductsOnPallet.Single(p => p.ProductId == 10);
			Assert.Equal(100, product1.Quantity);
			Assert.Equal(new DateOnly(2025, 2, 2), product1.BestBefore);			
		}

		[Fact]
		public async Task ShowDataToEdit_ShowPalletAsync_ReturnData()
		{
			//Arrange
			var palletId = "Q1000";
			//Act

			var result = await _palletService.GetPalletToEditAsync(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.ProductsOnPallet);
			Assert.Equal(2, result.ProductsOnPallet.Count);
			Assert.Equal(1, result.ReceiptId);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			var product1 = result.ProductsOnPallet.Single(p => p.ProductId == 10);
			Assert.Equal(50, product1.Quantity);
			Assert.Equal(new DateOnly(2026, 2, 2), product1.BestBefore);
			var product2 = result.ProductsOnPallet.Single(p => p.ProductId == 11);
			Assert.Equal(200, product2.Quantity);
			Assert.Equal(new DateOnly(2025, 2, 2), product2.BestBefore);
		}

	}
}
