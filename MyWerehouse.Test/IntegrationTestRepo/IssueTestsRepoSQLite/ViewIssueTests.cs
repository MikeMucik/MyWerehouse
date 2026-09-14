using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.IssueTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewIssueTests
	{
		private readonly IssueRepo _issueRepo;
		private readonly QueryTestSQLFixture _fixture;
		public ViewIssueTests(QueryTestSQLFixture fixture)
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
			var result = await _issueRepo.GetIssueByIdAsync(id, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal(11, result.ClientId);
		}

		[Fact]
		public async Task ShowListIssuesDate_GetIssuesByDates_ReturnList()
		{
			var sendDateStart = DateOnly.FromDateTime(TestDates.UtcNow);
			var sendDateEnd = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(1));
			//Act
			var result =await _issueRepo.GetIssuesByDates(sendDateStart, sendDateEnd, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.PalletNumber == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.PalletNumber == "Q1001"));
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
			var result = await _issueRepo.GetIssuesByIdsAsync(list, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.PalletNumber == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.PalletNumber == "Q1001"));
		}		
	}
}
