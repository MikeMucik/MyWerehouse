using MediatR;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Domain.Receiving.Filters;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.ReceiptTests.Integration
{
	[Collection("QueryCollection")]
	public class ReceiptViewIntegrationTests
	{
		private static readonly Guid ReceiptId1 =
			Guid.Parse("11111111-1111-1111-1111-111111111111");
		private static readonly Guid ReceiptId2 =
			Guid.Parse("21111111-1111-1111-1111-111111111111");

		private readonly IMediator _mediator;

		public ReceiptViewIntegrationTests(QueryTestSQLFixture fixture)
		{
			_mediator = fixture.Mediator;
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldReturnMatchingReceipts_WhenSkuExists()
		{
			// Arrange
			var query = CreateQueryForSku("fghtredfg");

			// Act
			var result = await _mediator.Send(query);

			// Assert
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Equal(2, result.Result.TotalCount);
			Assert.Equal(
				[ReceiptId1, ReceiptId2],
				result.Result.Items.Select(receipt => receipt.ReceiptId));
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldReturnFailure_WhenSkuIsNotAssignedToReceipt()
		{
			// Arrange
			var query = CreateQueryForSku("fghtredfg1");

			// Act
			var result = await _mediator.Send(query);

			// Assert
			Assert.False(result.IsSuccess);
			Assert.Equal("No receipts to display.", result.Error);
		}

		[Fact]
		public async Task GetReceiptsByFilter_ShouldReturnFailure_WhenSkuDoesNotExist()
		{
			// Arrange
			var query = CreateQueryForSku("999");

			// Act
			var result = await _mediator.Send(query);

			// Assert
			Assert.False(result.IsSuccess);
			Assert.Equal("No receipts to display.", result.Error);
		}

		private static GetReceiptsByFilterQuery CreateQueryForSku(string sku)
			=> new()
			{
				Filter = new IssueReceiptSearchFilter
				{
					SKU = sku
				},
				CurrentPage = 1,
				PageSize = 10
			};
	}
}
