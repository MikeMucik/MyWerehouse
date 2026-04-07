using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.IssueTestsRepoSQLite
{
	public class AddETCIssueTests : TestBase
	{

		[Fact]
		public void AddIssue_AddIssue_AddToCollection()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			
			var pallet1 = Pallet.CreateForTests("Q1010", DateTime.Now, 1, PalletStatus.Available, null, null);			
			
			var pallet2 = Pallet.CreateForTests("Q1011", DateTime.Now, 2, PalletStatus.Available, null, null);
			
			DbContext.Clients.Add(initailClient);			
			DbContext.Locations.AddRange(location1, location2);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var issueRepo = new IssueRepo(DbContext);
			//Act	
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, new DateTime(2025, 5, 5)
				, new DateTime(2025, 12, 15), "U003", IssueStatus.Pending, null);
			
			issueRepo.AddIssue(issue);
			issue.ReservePallet(pallet1, "U003");
			issue.ReservePallet(pallet2, "U003");
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Issues
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == issue.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U003", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.PalletNumber == "Q1010");
			Assert.Contains(result.Pallets, p => p.PalletNumber == "Q1011");

			foreach (var item in result.Pallets)
			{
				Assert.Equal(issue.Id, item.IssueId);
			}
			
		}
		[Fact]
		public void RemoveIssue_DeleteIssue_RemoveFromList()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			
			var pallet1 = Pallet.CreateForTests("Q1010", DateTime.Now, 1, PalletStatus.Available, null, null);
			
			var pallet2 = Pallet.CreateForTests("Q1011", DateTime.Now, 2, PalletStatus.Available, null, null);
			
			DbContext.Clients.Add(initailClient);
			
			DbContext.Locations.AddRange(location1, location2);			
			DbContext.Pallets.AddRange(pallet1, pallet2);			
			var issueRepo = new IssueRepo(DbContext);
			//Act		
			var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, 1, new DateTime(2025, 5, 5)
			, new DateTime(2025, 12, 15), "U003", IssueStatus.Pending, null);
			
			issueRepo.AddIssue(issue);
			issue.ReservePallet(pallet1, "U003");
			issue.ReservePallet(pallet2, "U003");
			DbContext.SaveChanges();
			//Act
			issueRepo.DeleteIssue(issue);
			DbContext.SaveChanges();
			//Assert
			var issueRemoved = DbContext.Issues.Find(issue.Id);
			Assert.Null(issueRemoved);
		}
	}
}
