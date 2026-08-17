using MediatR;
using MyWerehouse.Application.Inventories.Queries.GetInventory;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.InventoryTests.Integration
{
	[Collection("QueryCollection")]
	public class InventoryViewIntegrationTests
	{
		private readonly IMediator _mediator;

		public InventoryViewIntegrationTests(QueryTestSQLFixture fixture)
		{
			_mediator = fixture.Mediator;
		}

		[Fact]
		public async Task GetMissingInventory_ReturnNotFound()
		{
			//Arrange
			var missingProductId = Guid.NewGuid();
			//Act
			var result = await _mediator.Send(new GetInventoryQuery(missingProductId));
			//Assert
			Assert.False(result.IsSuccess);
			Assert.Null(result.Result);
			Assert.Equal(ErrorType.NotFound, result.ErrorType);
			Assert.Equal($"Inventory for product {missingProductId} does not exist.", result.Error);
		}
	}
}
