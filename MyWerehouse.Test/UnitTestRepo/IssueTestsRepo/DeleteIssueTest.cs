using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.IssueTestsRepo
{
	public class DeleteIssueTest :CommandTestBase
	{
		private readonly IssueRepo _issueRepo;
		public DeleteIssueTest() : base()
		{
			_issueRepo = new IssueRepo(_context);
		}
		[Fact]
		public void RemoveIssue_DeleteIssue_RemoveFromList()
		{
			//Arrange
			var id = 2;
			//Act
			_issueRepo.DeleteIssue(id);
			//Assert
			var issue = _context.Issues.Find(2);
			Assert.Null(issue);
		}
		[Fact]
		public async Task RemoveIssue_DeleteIssueAsync_RemoveFromList()
		{
			//Arrange
			var id = 2;
			//Act
			await _issueRepo.DeleteIssueAsync(id);
			//Assert
			var issue = _context.Issues.Find(2);
			Assert.Null(issue);
		}
	}
}
