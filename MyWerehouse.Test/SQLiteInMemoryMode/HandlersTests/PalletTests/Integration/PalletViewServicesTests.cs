using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr;
using MyWerehouse.Application.Pallets.Queries.GetPallet;
using MyWerehouse.Application.Pallets.Queries.GetPalletByPalletNumber;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletTests.Integration
{
	[Collection("QueryCollection")]
	public class PalletViewServicesTests
	{
		private readonly QueryTestSQLFixture _fixture;
		private readonly IMediator _mediator;
		public PalletViewServicesTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_mediator = _fixture.Mediator;
		}
		[Fact]
		public async Task GetPalletToEdit_ReturnUpdatePalletDTO()
		{
			//Arrange
			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");

			var palletId = "Q1001";
			var query = new GetPalletToEditQuery(palletGuid2);
			//Act
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var issueId1 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			var result = await _mediator.Send(query);
			Assert.NotNull(result);
			Assert.Single(result.Result.ProductsOnPallet);
			Assert.Equal(1, result.Result.LocationId);
			Assert.Equal(PalletStatus.OnHold, result.Result.Status);
			//Assert.Equal(receiptId1, result.Result.ReceiptId);
			//Assert.Equal(issueId1, result.Result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.Result.DateReceived);
			var product1 = result.Result.ProductsOnPallet.Single(p => p.ProductId == productId1);
			Assert.Equal(100, product1.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), product1.BestBefore);
		}

		[Fact]
		public async Task GetPalletToEdit_ShowDataToEdit_ReturnData()
		{
			//Arrange
			var palletGuid1 = Guid.Parse("00000000-0001-1111-0000-000000000000");

			var palletId = "Q1000";
			var query = new GetPalletToEditQuery(palletGuid1);
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			//Act

			var result = await _mediator.Send(query);
			//Assert
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var issueId1 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			Assert.NotNull(result);
			Assert.NotNull(result.Result.ProductsOnPallet);
			Assert.Equal(2, result.Result.ProductsOnPallet.Count);
			//Assert.Equal(receiptId1, result.Result.ReceiptId);
			//Assert.Equal(issueId1, result.Result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.Result.DateReceived);
			var product1 = result.Result.ProductsOnPallet.Single(p => p.ProductId == productId1);
			Assert.Equal(50, product1.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), product1.BestBefore);
			var product2 = result.Result.ProductsOnPallet.Single(p => p.ProductId == productId2);
			Assert.Equal(200, product2.Quantity);
			Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(366)), product2.BestBefore);
		}
		[Fact]
		public async Task FindPalletsByFilter_ReturnCollection()
		{
			//Arrange
			var filter = new PalletSearchFilter
			{
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))
			};
			//Act
			var query = new FindPalletsByFilterQuery
			//(filtr, 1, 1);
			{
				Filter = filter,
				PageSize = 1,
				CurrentPage = 1
			};
			var result =await _mediator.Send(query);
			//Assert
			Assert.NotEmpty(result.Result.Items);
		}
		[Fact]
		public void FindPallets_ReturnCollectionEmpty()
		{
			//Arrange
			var filter = new PalletSearchFilter
			{
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(36))
			};
			//Act
			var query = new FindPalletsByFilterQuery
			{
				Filter = filter,
				PageSize = 1,
				CurrentPage = 1
			};
			var result = _mediator.Send(query);
			//Assert
			Assert.False(result.Result.IsSuccess);
			Assert.Contains("Brak", result.Result.Error);
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
			Assert.NotNull(result.Result);
			var expected = _fixture.DbContext.Pallets
				.FirstOrDefault(p=>p.Id == palletGuid2);
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Null(result.Error);
			Assert.NotNull(result.Result);

			var pallet = result.Result;

			Assert.Equal(expected.Id, pallet.Id);
			Assert.Equal(expected.PalletNumber, pallet.PalletNumber);
			Assert.Equal(expected.DateReceived, pallet.DateReceived);
			Assert.Equal(expected.LocationId, pallet.LocationId);
			Assert.Equal(expected.Status, pallet.Status);

			Assert.Equal(expected.ReceiptId, pallet.ReceiptId);
			Assert.Equal(expected.Receipt.ReceiptNumber, pallet.ReceiptNumber);
			Assert.Equal(expected.IssueId, pallet.IssueId);
			Assert.Equal(expected.Issue.IssueNumber, pallet.IssueNumber);

			var product = Assert.Single(pallet.ProductsOnPallet);

			Assert.Equal(expected.ProductsOnPallet.First().ProductId, product.ProductId);
			Assert.Equal(expected.ProductsOnPallet.First().Product.SKU, product.ProductSKU);
			Assert.Equal(expected.ProductsOnPallet.First().Product.Name, product.ProductName);
			Assert.Equal(expected.ProductsOnPallet.First().Quantity, product.Quantity);
			Assert.Equal(expected.ProductsOnPallet.First().BestBefore, product.BestBefore);

			var movement = Assert.Single(pallet.PalletHistory);

			Assert.Equal("TestUser", movement.PerformedBy);
			Assert.Equal(ReasonForPallet.Moved, movement.Reason);
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
			Assert.True(result.IsSuccess);
			var expected = _fixture.DbContext.Pallets.FirstOrDefault(p=>p.PalletNumber == palletNumber);
			Assert.Equal(expected.Id, result.Result.Id);
		}
	}
}
