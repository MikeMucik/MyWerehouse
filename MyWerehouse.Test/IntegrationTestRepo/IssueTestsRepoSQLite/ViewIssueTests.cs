using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.IssueTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewIssueTests 
	{
		private readonly IssueRepo _issueRepo;
		private readonly QueryTestFixture _fixture;
		public ViewIssueTests(QueryTestFixture fixture)
		{
			_fixture = fixture;			
			_issueRepo = new IssueRepo(_fixture.DbContext);
		}		
		[Fact]
		public async Task ShowIssueById_GetIssueByIdAsync_ReturnIssue()
		{
			//Arrange
			var issueId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var id = issueId2;
			//Act
			var result = await _issueRepo.GetIssueByIdAsync(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal(11, result.ClientId);
		}
		[Fact]
		public void ShowListIssuesByClient_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ClientId = 11
			};
			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
		}
		[Fact]
		public void ShowListIssuesByProduct_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = 10
			};
			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
		}
		[Fact]
		public void ShowListIssuesDate_GetIssuesByFilter_ReturnList()
		{
			//Arrange
			var filter = new IssueReceiptSearchFilter
			{
				DateTimeStart = DateTime.UtcNow,
				DateTimeEnd = DateTime.UtcNow.AddDays(1)
			};

			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1001"));
		}
		[Fact]
		public async Task ShowListIssues_GetIssuesByIdsAsync_ReturnList()
		{
			//Arrange
			var issueId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");

			var issueId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var list = new List<Guid>
			{
				issueId1, issueId2
			};

			//Act
			var result =await _issueRepo.GetIssuesByIdsAsync(list);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1001"));
		}
		[Fact]
		public async Task ShowListIssues_GetPalletByIssueIdAsync_ReturnListPalletWithLocation()
		{
			//Arrange
			var receiptId2 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var issueId = receiptId2;

			//Act
			var result =await _issueRepo.GetPalletByIssueIdAsync(issueId);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.All(result, p => Assert.False(string.IsNullOrWhiteSpace(p.PalletId)));
			Assert.Contains(result, p => p.PalletId == "Q1000");
			Assert.Contains(result, p => p.PalletId == "Q1000"&&p.LocationId ==1);
			Assert.Contains(result, p => p.PalletId == "Q1001"&&p.LocationId ==1);
			Assert.Contains(result, p => p.PalletId == "Q2000"&&p.LocationId ==3);		
		}
	}
}
