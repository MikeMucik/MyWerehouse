using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	[Collection("QueryCollection")]
	public class ReceiptViewIntegrationService
	{
		private readonly ReceiptService _receiptService;
		private readonly IMapper _mapper;
		private readonly ReceiptRepo _receiptRepo;
		private readonly QueryTestFixture _fixture;

		public ReceiptViewIntegrationService(QueryTestFixture fixture)
		{
			_fixture = fixture;
			var mapperConfig = new MapperConfiguration(cfg =>
					{
						cfg.AddProfile<MappingProfile>();
					});
			_mapper = mapperConfig.CreateMapper();
			
			_receiptRepo = new ReceiptRepo(_fixture.DbContext);
			_receiptService = new ReceiptService(_receiptRepo, _mapper);
		}
		[Fact]
		public async Task GetReceiptDTOAsync_GetData_ReturnDTO()
		{
			//Arrange&Act			
			var result = await _receiptService.GetReceiptDTOAsync(1);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Id);
			Assert.Equal(10, result.ClientId);
			Assert.Equal("U001", result.PerformedBy);
			Assert.Equal(new DateTime(2023, 3, 3), result.ReceiptDateTime);
			
			// Jeśli DTO zawiera Pallets — ReceiptId = 1 ma palety Q1000 i Q1001
			Assert.NotNull(result.Pallets);
			Assert.Equal(2, result.Pallets.Count);
			Assert.Contains(result.Pallets, p => p.Id == "Q1000");
			Assert.Contains(result.Pallets, p => p.Id == "Q1001");

			// Można też sprawdzić przykładowy produkt z jednej palety
			var pallet = result.Pallets.First(p => p.Id == "Q1000");
			Assert.NotNull(pallet.ProductsOnPallet);
			Assert.Contains(pallet.ProductsOnPallet, pop => pop.ProductId == 10 && pop.Quantity == 50);

			// Dodatkowo: czy daty i statusy palet się zgadzają
			Assert.Equal(PalletStatus.Available, result.Pallets.First(p => p.Id == "Q1000").Status);
			Assert.Equal(PalletStatus.OnHold, result.Pallets.First(p => p.Id == "Q1001").Status);
		}
		[Fact]
		public async Task GetReceiptDTOAsync_GetData_ReturnNull()
		{
			//Arrange&Act			
			var result = await _receiptService.GetReceiptDTOAsync(999);
			//Assert
			Assert.Null(result);
		}
		//Testy multi
		[Theory]
		[InlineData(1, 10, "U001", "Q1000", "Q1001")] // ReceiptId = 1
		[InlineData(2, 11, "U002", "Q1002", "Q1010", "Q1100", "Q1101", "Q2000", "Q1200")] // ReceiptId = 2
		public async Task GetReceiptDTOAsync_ReturnsExpectedData(
			int receiptId,
			int expectedClientId,
			string expectedUser,
			//string expectedClientName,
			params string[] expectedPalletIds)
		{
			// Arrange & Act
			var result = await _receiptService.GetReceiptDTOAsync(receiptId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(receiptId, result.Id);
			Assert.Equal(expectedClientId, result.ClientId);
			Assert.Equal(expectedUser, result.PerformedBy);
			//Assert.Equal(expectedClientName, result.Client.Name);

			// Sprawdź czy wszystkie oczekiwane palety są zwrócone
			Assert.NotNull(result.Pallets);
			foreach (var palletId in expectedPalletIds)
			{
				Assert.Contains(result.Pallets, p => p.Id == palletId);
			}
		}

	}
}
