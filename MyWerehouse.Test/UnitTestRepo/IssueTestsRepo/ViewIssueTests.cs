using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.IssueTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewIssueTests : CommandTestBase
	{
		private readonly IssueRepo _issueRepo;
		public ViewIssueTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_issueRepo = new IssueRepo(_context);
		}
		[Fact]
		public void ShowIssueById_GetIssueFilter_ReturnIssue()
		{
			//Arrange
			var id = 2;
			//Act
			var result = _issueRepo.GetIssueById(id);
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
			Assert.Contains(result, p => p.Pallets.Any(i=>i.Id == "Q1000"));
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
				DateTimeEnd = new DateTime(2025, 6, 6),
				DateTimeStart = new DateTime(2020, 1, 1),
			};
			
			//Act
			var result = _issueRepo.GetIssuesByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1000"));
			Assert.Contains(result, p => p.Pallets.Any(i => i.Id == "Q1001"));
		}
	}
}
