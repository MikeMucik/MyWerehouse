using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Receipts.Queries.GetReceipt;
using MyWerehouse.Application.Receipts.Queries.GetReceipts;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	[Collection("QueryCollection")]
	public class ReceiptViewIntegrationTests
	{		
		private readonly QueryTestFixture _fixture;
		private readonly IMediator _mediator;

		public ReceiptViewIntegrationTests(QueryTestFixture fixture)
		{
			_fixture = fixture;			
			_mediator = _fixture.Mediator;			
		}
		[Fact]
		public async Task GetReceiptDTOAsync_GetData_ReturnDTO()
		{
			//Arrange&Act
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");

			var query = new GetReceiptByIdQuery(receiptId1);
						
			var result = await _mediator.Send(query);
			
			//Assert
			Assert.NotNull(result);
			Assert.Equal(receiptId1, result.Result.ReceiptId);
			Assert.Equal(10, result.Result.ClientId);
			Assert.Equal("U001", result.Result.PerformedBy);
			Assert.Equal(new DateTime(2023, 3, 3), result.Result.ReceiptDateTime);
			
			// Jeśli DTO zawiera Pallets — ReceiptId = 1 ma palety Q1000 i Q1001
			Assert.NotNull(result.Result.Pallets);
			Assert.Equal(2, result.Result.Pallets.Count);
			Assert.Contains(result.Result.Pallets, p => p.PalletNumber == "Q1000");
			Assert.Contains(result.Result.Pallets, p => p.PalletNumber == "Q1001");

			// Można też sprawdzić przykładowy produkt z jednej palety
			var pallet = result.Result.Pallets.First(p => p.PalletNumber == "Q1000");
			Assert.NotNull(pallet.ProductsOnPallet);
			Assert.Contains(pallet.ProductsOnPallet, pop => pop.ProductId == productId1 && pop.Quantity == 50);

			// Dodatkowo: czy daty i statusy palet się zgadzają
			Assert.Equal(PalletStatus.Available, result.Result.Pallets.First(p => p.PalletNumber == "Q1000").Status);
			Assert.Equal(PalletStatus.OnHold, result.Result.Pallets.First(p => p.PalletNumber == "Q1001").Status);
		}
		[Fact]
		public async Task GetReceiptDTOAsync_GetData_ReturnNull()
		{
			//Arrange&Act		
			var receiptId9 = Guid.Parse("99111111-1111-1111-1111-111111111111");	
			var query = new GetReceiptByIdQuery(receiptId9);

			var result = await _mediator.Send(query);
			//var rresult = await Assert.ThrowsAsync<NotFoundReceiptException>(async () => await _mediator.Send(query));
			//Assert
			Assert.NotNull(result);
			//Assert.NotNull(rresult);
			Assert.Contains($"Przyjęcie o numerze {receiptId9} nie zostało znalezione.", result.Error);
		}

		//Testy multi
		[Theory]
		//var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
		[InlineData("11111111-1111-1111-1111-111111111111", 10, "U001", "Q1000", "Q1001")] // ReceiptId = 1
		[InlineData("21111111-1111-1111-1111-111111111111", 11, "U002", "Q1002", "Q1010", "Q1100", "Q1101", "Q2000", "Q1200")] // ReceiptId = 2
		public async Task GetReceiptDTOAsync_ReturnsExpectedData(
			Guid receiptId,
			int expectedClientId,
			string expectedUser,
			//string expectedClientName,
			params string[] expectedPalletIds)
		{
			// Arrange & Act
			var query = new GetReceiptByIdQuery(receiptId);
						
			var result = await _mediator.Send(query);
			// Assert
			Assert.NotNull(result);
			Assert.Equal(receiptId, result.Result.ReceiptId);
			Assert.Equal(expectedClientId, result.Result.ClientId);
			Assert.Equal(expectedUser, result.Result.PerformedBy);
			//Assert.Equal(expectedClientName, result.Client.Name);

			// Sprawdź czy wszystkie oczekiwane palety są zwrócone
			Assert.NotNull(result.Result.Pallets);
			foreach (var palletId in expectedPalletIds)
			{
				Assert.Contains(result.Result.Pallets, p => p.PalletNumber == palletId);
			}
		}
		[Fact]
		public async Task GetReceiptsDTOAsync_GetData_ReturnListDTO()
		{
			//Arrange&Act		
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = Guid.Parse("00000000-0000-0000-0001-000000000000")
			};
			//var result = await _receiptService.GetReceiptDTOsAsync(filter);
			var query = new GetReceiptsQuery(filter,1,2);

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result.Result.Dtos); // should return some data

			// Verify that receipts 1 and 2 are included
			var receiptIds = result.Result.Dtos.Select(r => r.ReceiptId).ToList();
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receiptId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			Assert.Contains(receiptId1, receiptIds);
			Assert.Contains(receiptId2, receiptIds);			
			// Optionally: ensure no duplicates and correct client mapping
			Assert.All(result.Result.Dtos, r =>
			{
				Assert.True(r.ClientId == 10 || r.ClientId == 11);
				//Assert.True(r.ReceiptId > 0);
			});
		}

		//[Theory]
		//[InlineData(10, new[] { 1, 2 })]   // Product 10 appears in Receipts 1 & 2
		//[InlineData(11, new[] { 1 })]      // Product 11 appears in Receipt 2
		//[InlineData(999, new int[0])]      // Product not existing -> expect empty list
		//public async Task GetReceiptsDTOAsync_ByProductId_ReturnsExpectedReceipts(
		//	int productId,
		//	int[] expectedReceiptIds)
		//{
		//	// Arrange
		//	var filter = new IssueReceiptSearchFilter
		//	{
		//		ProductId = productId
		//	};

		//	// Act
		//	//var result = await _receiptService.GetReceiptDTOsAsync(filter);
		//	var query = new GetReceiptsQuery(filter);

		//	var result = await _mediator.Send(query);
		//	// Assert
		//	Assert.NotNull(result);

		//	if (expectedReceiptIds.Length == 0)
		//	{
		//		Assert.Empty(result);
		//		return;
		//	}

		//	var actualReceiptIds = result.Select(r => r.Id).ToList();

		//	foreach (var expectedId in expectedReceiptIds)
		//		Assert.Contains(expectedId, actualReceiptIds);

		//	Assert.All(result, r =>
		//	{
		//		Assert.True(r.Id > 0);
		//		Assert.True(r.ClientId > 0);
		//		Assert.NotNull(r.Pallets);
		//	});
		//}







		//[Theory]
		//[InlineData(10, 2, new[] { 1, 2 })] // ProductId=10 → receipts 1 and 2
		//[InlineData(11, 2, new[] {1, 2 })]    // ProductId=11 → receipts 1 and 2
		//[InlineData(989, 0, new int[0])]    // ProductId=989 → no receipts
		//public async Task GetReceiptsDTOAsync_FilterByProductId_ReturnsExpectedReceipts(
	 //  int productId, int expectedCount, int[] expectedReceiptIds)
		//{
		//	// Arrange
		//	var filter = new IssueReceiptSearchFilter { ProductId = productId };

		//	// Act
		//	//var result = await _receiptService.GetReceiptDTOsAsync(filter);
		//	var query = new GetReceiptsQuery(filter);

		//	var result = await _mediator.Send(query);
		//	// Assert
		//	Assert.NotNull(result);
		//	Assert.Equal(expectedCount, result.Count);

		//	// Verify IDs if any expected
		//	if (expectedReceiptIds.Any())
		//	{
		//		var actualIds = result.Select(r => r.ReceiptId).OrderBy(x => x).ToList();
		//		Assert.Equal(expectedReceiptIds.OrderBy(x => x).ToList(), actualIds);

		//		// Check for each DTO basic correctness
		//		foreach (var dto in result)
		//		{
		//			Assert.True(dto.ClientId > 0);
		//			Assert.False(string.IsNullOrWhiteSpace(dto.PerformedBy));
		//			Assert.NotEqual(default(DateTime), dto.ReceiptDateTime);

		//			// Check at least one product on any pallet matches the searched ProductId
		//			var anyMatch = dto.Pallets
		//				.SelectMany(p => p.ProductsOnPallet)
		//				.Any(prod => prod.ProductId == productId);

		//			Assert.True(anyMatch,
		//				$"Receipt {dto.ReceiptId} should contain product {productId}, but none found.");
		//		}
		//	}
		//}
	}
}
