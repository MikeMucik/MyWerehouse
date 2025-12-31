using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ReceiptTestRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewReceiptTests

	{		
		private readonly ReceiptRepo _receiptRepo;
		private readonly QueryTestFixture _fixture;
		public ViewReceiptTests(QueryTestFixture fixture)
		{
			_fixture = fixture;			
			_receiptRepo = new ReceiptRepo(_fixture.DbContext);
		}		
		[Fact]
		public async Task ShowReceiptById_GetReceiptByIdAsync_ReturnList()
		{
			//Arrange
			var id = 1;
			//Act
			var result =await _receiptRepo.GetReceiptByIdAsync(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal(10, result.ClientId);
		}
		[Fact]
		public void ShowListReceiptsByClient_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = 10
			};
			//Act
			var result = _receiptRepo.GetReceiptByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
		}
		[Fact]
		public void ShowListReceiptsByProduct_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = 10
			};
			//Act
			var result = _receiptRepo.GetReceiptByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
		}
		[Fact]
		public void ShowListReceiptsDate_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				DateTimeEnd = new DateTime(2025, 6, 6),
				DateTimeStart = new DateTime(2020, 1, 1),
			};

			//Act
			var result = _receiptRepo.GetReceiptByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1001"));
		}
	}
}
