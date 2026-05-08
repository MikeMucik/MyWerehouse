using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PickingTaskTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewPickingTaskRepoTests
	{
		private readonly PickingTaskRepo _pickingTaskRepo;
		private readonly QueryTestFixture _fixture;
		public ViewPickingTaskRepoTests(QueryTestFixture fixture)
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
			var result = await _pickingTaskRepo.GetPickingTaskAsync(pickingTaskId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(receiptId2, result.IssueId);
			Assert.Equal(20, result.RequestedQuantity);
			Assert.Equal(3, result.VirtualPallet.LocationId);
		}
		[Fact]
		public void ByVirtualPalletAndDatePicking_GetPickingTaskListAsync_ReturnList()
		{
			//Arrange
			var vpId1 = Guid.Parse("22222222-1111-2222-1111-111111111111");
			var virtualPalletId = 1;
			var date = DateTime.UtcNow;
			//Act
			var result =  _pickingTaskRepo.GetPickingTaskList(vpId1, date);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result); // coś zostało znalezione

			// wszystkie alokacje mają właściwy VirtualPallet
			Assert.All(result, a => Assert.Equal(vpId1, a.VirtualPalletId));

			// wszystkie alokacje mają status Allocated
			Assert.All(result, a => Assert.Equal(PickingStatus.Allocated, a.PickingStatus));

			// wszystkie dotyczą zleceń na dziś lub jutro
			Assert.All(result, a =>
			{
				var sendDate = a.Issue.IssueDateTimeSend.Date;
				Assert.Contains(sendDate, new[] { date.Date, date.AddDays(1).Date });
			});
		}
		[Fact]
		public async Task ByIssueAndProductId_GetPickingTasksByIssueIdProductIdAsync_ReturnList()
		{
			//Arrange
			var receiptId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var issueId = receiptId2;
			var productId = Guid.Parse("00000000-0000-0000-0002-000000000000");;
			//Act
			var result = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issueId, productId);
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
			var result = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issueId);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(6,result.Count);
			Assert.All(result, a => Assert.Equal(issueId, a.IssueId));
		}
		[Fact]
		public async Task ByProductIdAndDates_GetPickingTasksProductIdAsync_ReturnList()
		{
			//var productId = 11;
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			var dateStart = DateTime.UtcNow;
			var dateEnd = DateTime.UtcNow.AddDays(1);
			//Act
			var result = await _pickingTaskRepo.GetPickingTasksProductIdAsync(productId2, dateStart, dateEnd);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(3, result.Count);
			Assert.All(result, a => Assert.Equal(productId2, a.ProductId));

		}
	}
}
