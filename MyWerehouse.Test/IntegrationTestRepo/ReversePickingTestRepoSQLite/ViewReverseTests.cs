using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ReversePickingTestRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewReverseTests
	{
		private readonly ReversePickingRepo _reversePickingRepo;
		private readonly QueryTestFixture _fixture;
		public ViewReverseTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_reversePickingRepo = new ReversePickingRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task ShowReversePicking_GetReversePickingAsync_ReturnRecord()
		{
			//Arrange
			var id = 1;
			//Act
			var result = await _reversePickingRepo.GetReversePickingAsync(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.PickingTaskId);
			Assert.Equal("Q5000", result.PickingPalletId);
			Assert.Equal(10, result.ProductId);
			Assert.Equal(10, result.Quantity);
			Assert.Equal("UserR", result.UserId);
		}
		//[Fact]
		//public async Task ShowReversePicking_GetReversePickingAsync_ReturnNull()
		//{
		//	//Arrange
		//	var id = 1;
		//	//Act
		//	var result = await _reversePickingRepo.GetReversePickingAsync(id);
		//	//Assert
		//	Assert.NotNull(result);
		//	Assert.Equal(2, result.PickingTaskId);
		//	Assert.Equal("Q5000", result.PickingPalletId);
		//	Assert.Equal(10, result.ProductId);
		//	Assert.Equal(10, result.Quantity);
		//	Assert.Equal("UserR", result.UserId);
		//}
		[Fact]
		public void ShowReversePicking_GetReversePickings_ReturnRecords()
		{
			//Arrange&Act
			var result = _reversePickingRepo.GetReversePickings();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
		}
	}
}
