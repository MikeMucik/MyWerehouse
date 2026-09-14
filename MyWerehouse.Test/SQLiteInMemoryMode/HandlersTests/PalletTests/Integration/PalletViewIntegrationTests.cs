using MediatR;
using MyWerehouse.Application.Pallets.Queries.FindPalletsByFilter;
using MyWerehouse.Application.Pallets.Queries.GetPallet;
using MyWerehouse.Application.Pallets.Queries.GetPalletByPalletNumber;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletTests.Integration
{
	[Collection("QueryCollection")]
	public class PalletViewIntegrationTests
	{
		private readonly QueryTestSQLFixture _fixture;
		private readonly IMediator _mediator;
		public PalletViewIntegrationTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_mediator = _fixture.Mediator;
		}
		[Fact]
		public async Task GetPalletToEdit_ReturnUpdatePalletDTO()
		{
			//Arrange
			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");

			var query = new GetPalletToEditQuery(palletGuid2);
			//Act
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var issueId1 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			var result = await _mediator.Send(query);
			Assert.NotNull(result);
			Assert.NotNull(result.Result);
			Assert.Single(result.Result.ProductsOnPallet);
			//Assert.Equal(1, result.Result.LocationId);
			Assert.Equal(PalletStatus.OnHold, result.Result.Status);
			Assert.Equal(new DateTime(2020, 1, 1), result.Result.DateReceived);
			var product1 = result.Result.ProductsOnPallet.Single(p => p.ProductId == productId1);
			Assert.Equal(100, product1.Quantity);
			Assert.Equal(DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366)), product1.BestBefore);
		}

		[Fact]
		public async Task GetPalletToEdit_ShowDataToEdit_ReturnData()
		{
			//Arrange
			var palletGuid1 = Guid.Parse("00000000-0001-1111-0000-000000000000");

			var query = new GetPalletToEditQuery(palletGuid1);
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			//Act

			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.ProductsOnPallet);
			Assert.Equal(2, result.Result.ProductsOnPallet.Count);
			Assert.Equal(new DateTime(2020, 1, 1), result.Result.DateReceived);
			var product1 = result.Result.ProductsOnPallet.Single(p => p.ProductId == productId1);
			Assert.Equal(50, product1.Quantity);
			Assert.Equal(DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366)), product1.BestBefore);
			var product2 = result.Result.ProductsOnPallet.Single(p => p.ProductId == productId2);
			Assert.Equal(200, product2.Quantity);
			Assert.Equal(DateOnly.FromDateTime(TestDates.TodayDateTime.AddDays(366)), product2.BestBefore);
		}
		[Fact]
		public async Task FindPalletsByFilter_ReturnCollection()
		{
			//Arrange
			var filter = new PalletSearchFilter
			{
				BestBeforeFrom = DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(3))
			};
			//Act
			var query = new FindPalletsByFilterQuery
			{
				Filter = filter,
				PageSize = 1,
				CurrentPage = 1
			};
			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.Result);
			Assert.NotEmpty(result.Result.Items);
		}
		[Fact]
		public async Task FindPallets_ReturnCollectionEmpty()
		{
			//Arrange
			var filter = new PalletSearchFilter
			{
				BestBeforeFrom = DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(36))
			};
			//Act
			var query = new FindPalletsByFilterQuery
			{
				Filter = filter,
				PageSize = 1,
				CurrentPage = 1
			};
			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("No pallets match the specified criteria.", result.Error);
		}
		[Fact]
		public async Task GetPallet_ReturnFullInfo()
		{
			//Arrange
			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");
			//Act
			var query = new GetPalletQuery(palletGuid2);
			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Null(result.Error);
			Assert.NotNull(result.Result);
			var pallet = result.Result;
			Assert.Equal("Q1001", pallet.PalletNumber);
			Assert.Equal(1, pallet.ReceiptNumber);
			Assert.Equal(2,pallet.IssueNumber);

			var product = Assert.Single(pallet.ProductsOnPallet);
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			Assert.Equal(productId1, product.ProductId);
			Assert.Equal("0987654321", product.ProductSKU);
			Assert.Equal(100, product.Quantity);

			var movement = Assert.Single(pallet.PalletHistory);

			Assert.Equal("TestUser", movement.PerformedBy);
			Assert.Equal(ReasonForPallet.Moved, movement.Reason);
		}
		[Fact]
		public async Task GetMissingPallet_ReturnNotFound()
		{
			//Arrange
			var missingPalletId = Guid.NewGuid();
			//Act
			var result = await _mediator.Send(new GetPalletQuery(missingPalletId));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Null(result.Result);
			Assert.Equal(ErrorType.NotFound, result.ErrorType);
			Assert.Equal($"Pallet {missingPalletId} does not exist.", result.Error);
		}
		[Fact]
		public async Task GetPalletByPalletnumber_ReturnPallet()
		{
			//Arrange
			var palletNumber = "Q1000";
			//Act
			var query = new GetPalletByPalletNumberQuery(palletNumber);
			var result = await _mediator.Send(query);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.Result);
			Assert.True(result.IsSuccess);
			var expected = _fixture.DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == palletNumber);
			Assert.NotNull(expected);
			Assert.Equal(expected.Id, result.Result.Id);
		}
	}
}
