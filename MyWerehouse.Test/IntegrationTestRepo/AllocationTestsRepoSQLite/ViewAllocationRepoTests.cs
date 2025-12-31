using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.AllocationTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewAllocationRepoTests
	{
		private readonly AllocationRepo _allocationRepo;
		private readonly QueryTestFixture _fixture;
		public ViewAllocationRepoTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_allocationRepo = new AllocationRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task TakeAllocationById_GetAllocationAsync_ReturnProperData()
		{
			//Arrange
			var allocationId = 1;
			//Act
			var result = await _allocationRepo.GetAllocationAsync(allocationId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(20, result.Quantity);
			Assert.Equal(3, result.VirtualPallet.LocationId);
		}
		[Fact]
		public async Task ByVirtualPalletAndDatePicking_GetAllocationListAsync_ReturnList()
		{
			//Arrange
			var virtualPalletId = 1;
			var date = new DateTime(2025, 5, 5);
			//var checkId = await _allocationRepo.GetAllocationAsync(1);
			//Act
			var result = await _allocationRepo.GetAllocationListAsync(virtualPalletId, date);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result); // coś zostało znalezione

			// wszystkie alokacje mają właściwy VirtualPallet
			Assert.All(result, a => Assert.Equal(virtualPalletId, a.VirtualPalletId));

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
		public async Task ByIssueAndProductId_GetAllocationsByIssueIdProductIdAsync_ReturnList()
		{
			//Arrange
			var issueId = 2;
			var productId = 11;
			//Act
			var result = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issueId, productId);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);

			Assert.All(result, a=> Assert.Equal(issueId, a.IssueId));
			Assert.All(result, a=> Assert.Equal(productId, a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId));
		}
		[Fact]
		public async Task ByIssue_GetAllocationsByIssueIdAsync_ReturnList()
		{
			var issueId = 2;			
			//Act
			var result = await _allocationRepo.GetAllocationsByIssueIdAsync(issueId);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(5,result.Count);
			Assert.All(result, a => Assert.Equal(issueId, a.IssueId));
		}
		[Fact]
		public async Task ByProductIdAndDates_GetAllocationsProductIdAsync_ReturnList()
		{
			var productId = 11;
			var dateStart = new DateTime(2025, 4,6);
			var dateEnd = new DateTime(2025, 5, 7);
			//Act
			var result = await _allocationRepo.GetAllocationsProductIdAsync(productId, dateStart, dateEnd);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count);
			Assert.All(result, a => Assert.Equal(productId, a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId));

		}
	}
}
