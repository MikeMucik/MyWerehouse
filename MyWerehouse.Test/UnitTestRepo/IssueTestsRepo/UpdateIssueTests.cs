using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.UnitTestRepo.IssueTestsRepo
{
	public class UpdateIssueTests
	{
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public UpdateIssueTests() 			
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("TestDataBase")
				.Options;
		}
		[Fact]
		public void UpdateIssueData_UpdateIssue_ChangeData()
		{
			//Arrange
			var updatingIssue = new Issue
			{
				Id = 10,
				ClientId = 1,
				IssueDateTime = new DateTime(2025, 4, 4),
				PerformedBy = "U0099",
				Pallets =new List<Pallet>{
					new Pallet { Id = "999" }, new Pallet {Id = "998"}
				}
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Issues.Add(updatingIssue);
			arrangeContext.SaveChanges();
			//Act
			var updatedIssue = new Issue
			{
				Id = 10,
				ClientId = 1,
				IssueDateTime = new DateTime(2026, 3, 3),
				PerformedBy = "U0098",
				Pallets = new List<Pallet>{
					new Pallet { Id = "999" }, 
					new Pallet {Id = "998"},
					new Pallet{Id ="997"}
				}
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new IssueRepo(actContext);
				repo.UpdateIssue(updatedIssue);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Issues.FirstOrDefault(x => x.Id == updatingIssue.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedIssue.PerformedBy, result.PerformedBy);
				Assert.Equal(updatedIssue.IssueDateTime, result.IssueDateTime);				
			}
		}
	}
}
