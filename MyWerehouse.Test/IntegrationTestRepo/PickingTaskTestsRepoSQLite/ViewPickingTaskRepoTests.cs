using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PickingTaskTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewPickingTaskRepoTests
	{
		private readonly PickingTaskRepo _pickingTaskRepo;
		private readonly QueryTestSQLFixture _fixture;
		public ViewPickingTaskRepoTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_pickingTaskRepo = new PickingTaskRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task TakePickingTaskById_GetPickingTaskAsync_ReturnProperData()
		{
			//Arrange
			var receiptId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var pickingId1 = Guid.Parse("11111111-1111-2222-1111-111111111111");
			var pickingTaskId = pickingId1;
			//Act
			var result = await _pickingTaskRepo.GetPickingTaskAsync(pickingTaskId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(receiptId2, result.IssueId);
			Assert.Equal(20, result.RequestedQuantity);
			Assert.NotNull(result.VirtualPallet);
			Assert.Equal(3, result.VirtualPallet.LocationId);
		}
		
		[Fact]
		public async Task ByIssueAndProductId_GetPickingTasksByIssueIdProductIdAsync_ReturnList()
		{
			//Arrange
			var receiptId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var issueId = receiptId2;
			var productId = Guid.Parse("00000000-0000-0000-0002-000000000000");;
			//Act
			var result = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issueId, productId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);

			Assert.All(result, a=> Assert.Equal(issueId, a.IssueId));
			Assert.All(result, a=> Assert.Equal(productId, a.ProductId));
		}
		[Fact]
		public async Task ByIssue_GetPickingTasksByIssueIdAsync_ReturnList()
		{
			//Arrange
			var receiptId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var issueId = receiptId2;
			//Act
			var result = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issueId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(4, result.Count);
			Assert.All(result, a => Assert.Equal(issueId, a.IssueId));
			Assert.All(result, a => Assert.True(
				a.PickingStatus == PickingStatus.Allocated ||
				a.PickingStatus == PickingStatus.CorrectionPicking));
		}		
	}
}
