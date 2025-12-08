using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.IssueItemTestsRepo
{
	[Collection("QueryCollection")]
	public class ViewIssueItemTests //: TestBase
	{
		private readonly IssueItemRepo _issueItemRepo;
		private readonly QueryTestFixture _fixture;
		public ViewIssueItemTests(QueryTestFixture fixture)
		{			
			_fixture = fixture;
			_issueItemRepo = new IssueItemRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task GetInfo_GetQuantityByIssueAndProduct_ReturnQuantity()
		{
			//Arrange
			var issueId = 2;			
			var issue = _fixture.DbContext.Issues.Find(issueId);
			//Act
			
			var productId = 10;
			var result = await _issueItemRepo.GetQuantityByIssueAndProduct(issue, productId);
			
			//Assert			
			Assert.Equal(150, result);
		}
	}
}
