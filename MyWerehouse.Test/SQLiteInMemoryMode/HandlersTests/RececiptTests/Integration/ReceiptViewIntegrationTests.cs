using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Receipts.Queries.GetReceiptById;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.RececiptTests.Integration
{
	[Collection("QueryCollection")]
	public class ReceiptViewIntegrationTests
	{		
		private readonly QueryTestSQLFixture _fixture;
		private readonly IMediator _mediator;

		public ReceiptViewIntegrationTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;			
			_mediator = _fixture.Mediator;			
		}
		[Fact]
		public async Task GetReceiptById_ReturnDTO()
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
		public async Task GetReceiptById_ReturnNull_WhenReceiptNotExist()
		{
			//Arrange&Act		
			var receiptId9 = Guid.Parse("99111111-1111-1111-1111-111111111111");	
			var query = new GetReceiptByIdQuery(receiptId9);

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			//Assert.NotNull(rresult);
			Assert.Contains($"Przyjęcie o numerze {receiptId9} nie zostało znalezione.", result.Error);
		}

		//Testy multi
		[Theory]
		[InlineData("11111111-1111-1111-1111-111111111111", 10, "ClientTest", "U001", "Q1000", "Q1001")] // ReceiptId = 1
		[InlineData("21111111-1111-1111-1111-111111111111", 11, "ClientTest1", "U002", "Q1002", "Q1010", "Q1100", "Q1101", "Q2000", "Q1200")] // ReceiptId = 2
		public async Task GetReceiptById_ReturnsExpectedData(
			Guid receiptId,
			int expectedClientId,
			string expectedClientName,
			string expectedUser,
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
			Assert.Equal(expectedClientName, result.Result.ClientName);

			// Sprawdź czy wszystkie oczekiwane palety są zwrócone
			Assert.NotNull(result.Result.Pallets);
			foreach (var palletId in expectedPalletIds)
			{
				Assert.Contains(result.Result.Pallets, p => p.PalletNumber == palletId);
			}
		}
		[Fact]
		public async Task GetReceiptsByFilter_ReturnListDTO()
		{
			//Arrange&Act		
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = Guid.Parse("00000000-0000-0000-0001-000000000000")
			};
			var query = new GetReceiptsByFilterQuery
			{
				Filter = filter,
				CurrentPage = 1,
				PageSize = 2,
			};				

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.Result); 
			Assert.NotEmpty(result.Result.Items); 

			// Verify that receipts 1 and 2 are included
			var receiptIds = result.Result.Items.Select(r => r.ReceiptId).ToList();
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receiptId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			Assert.Contains(receiptId1, receiptIds);
			Assert.Contains(receiptId2, receiptIds);
			// Optionally: ensure no duplicates and correct client mapping
			Assert.All(result.Result.Items, r =>
			{
				Assert.True(r.ClientId == 10 || r.ClientId == 11);
			});
		}

		public static IEnumerable<object[]> GetReceiptsByProductSkuCases()
		{
			yield return new object[]
			{
		"fghtredfg",
		new[]
		{
			Guid.Parse("11111111-1111-1111-1111-111111111111"),
			Guid.Parse("21111111-1111-1111-1111-111111111111")
		}
			};

			yield return new object[]
			{
		"fghtredfg1",
		new[]
		{
			Guid.Parse("11111111-1111-1111-1111-111111111111"),
			Guid.Parse("21111111-1111-1111-1111-111111111111")
		}
			};

			yield return new object[]
			{
		"999",
		Array.Empty<Guid>()
			};
		}

		[Theory]
		[MemberData(nameof(GetReceiptsByProductSkuCases))]
		public async Task GetReceiptsByFilter_ReturnsExpectedReceipts_ByProductId(
			string sku,
			Guid[] expectedReceiptIds)
		{
			// Arrange
			var filter = new IssueReceiptSearchFilter
			{
				SKU = sku
			};

			// Act
			var query = new GetReceiptsByFilterQuery
			{
				Filter = filter,
				CurrentPage = 1,
				PageSize = 10,
			};

			var result = await _mediator.Send(query);
			// Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Items);
			Assert.IsType<PagedResult<ReceiptSimplyDTO>>(result.Result);
			// Verify IDs if any expected
			if (expectedReceiptIds.Any())
			{
				var actualIds = result.Result.Items
					.Select(r=>r.ReceiptId)
					.OrderBy(x => x)
					.ToList();
				var expectectedIds = expectedReceiptIds
					.OrderBy(x=>x)
					.ToList();
				Assert.Equal(expectectedIds, actualIds);

				// Check for each DTO basic correctness
				foreach (var dto in result.Result.Items)
				{
					Assert.True(dto.ClientId > 0);
					Assert.False(string.IsNullOrWhiteSpace(dto.PerformedBy));
					Assert.NotEqual(default(DateTime), dto.ReceiptDateTime);
				}
			}
		}
	}
}
