//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Metrics;
//using System.Linq;
//using System.Net;
//using System.Numerics;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions.Execution;
//using MyWerehouse.Domain.Interfaces;
//using MyWerehouse.Domain.Models;
//using MyWerehouse.Test.SQLiteInMemoryMode;

//namespace MyWerehouse.Test.IntegrationTestRepo.HistoryIssueRepo
//{
//	public class AddHistoryIssueTests : TestBase
//	{
//		[Fact]
//		public void AddRecord_AddPalletMovement_AddToList()
//		{
//			//Arrange
//			var address = new Address
//			{
//				City = "Warsaw",
//				Country = "Poland",
//				PostalCode = "00-999",
//				StreetName = "Wiejska",
//				Phone = 4444444,
//				Region = "Mazowieckie",
//				StreetNumber = "23/3"
//			}
//			;
//			var initailClient = new Client
//			{
//				Name = "TestCompany",
//				Email = "123@op.pl",
//				Description = "Description",
//				FullName = "FullNameCompany",
//				Addresses = new List<Address> { address }
//			};
//			var initialCategory = new Category
//			{
//				Name = "name",
//				IsDeleted = false
//			};
//			var product = new Product
//			{
//				Name = "TestFull",
//				SKU = "123",
//				AddedItemAd = new DateTime(2024, 1, 1),
//				Category = initialCategory,
//				IsDeleted = false,
//				CartonsPerPallet = 10,
//			};
//			var location1 = new Location
//			{
//				Aisle = 1,
//				Bay = 1,
//				Height = 1,
//				Position = 1
//			};
//			var location2 = new Location
//			{
//				Aisle = 2,
//				Bay = 1,
//				Height = 1,
//				Position = 1
//			};
//			var location3 = new Location
//			{
//				Aisle = 3,
//				Bay = 1,
//				Height = 1,
//				Position = 1
//			};
//			var pallet1 = new Pallet
//			{
//				Id = "Q1000",
//				DateReceived = DateTime.Now,
//				LocationId = 1,
//				Status = PalletStatus.Available,
//				//ReceiptId = 10,
//			};
//			var issue = new Issue
//			{
//				a
//			};
//			DbContext.Clients.Add(initailClient);
//			DbContext.Categories.Add(initialCategory);
//			DbContext.Products.Add(product);
//			DbContext.Locations.AddRange(location1, location2, location3);
//			DbContext.Pallets.AddRange(pallet1);
//			DbContext.Issues.AddRange(issue);
//			DbContext.SaveChanges();
//			var historyIssue = new HistoryIssue
//			{
//				Issue = issue,
//				ClientId = initailClient.Id,
//				DateTime = DateTime.Now,
//				//Items
//				Details = new List<HistoryIssueDetail>
//				{
//					new HistoryIssueDetail
//					{
//						PalletId = pallet1.Id,
//						LocationId = location1.Id
//					}
//				},
//				StatusAfter = IssueStatus.Archived,
//				PerformedBy = "U001"
//			};
//			var historyIssueRepo = new MyWerehouse.Infrastructure.Repositories.HistoryIssueRepo(DbContext);
//			//Act
//			historyIssueRepo.AddHistoryIssue(historyIssue);
//			DbContext.SaveChanges();
//			//Assert			
			
//		}
//	}
//}
