using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
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
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};

			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			
			var availablePallets = new List<Pallet>
			{
				new Pallet
				{
					Id = "P1",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location1,
					Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
				}
			},
				new Pallet
				{
					Id = "P2",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location2,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
					}
				},
				new Pallet
				{
					Id = "P3",
					DateReceived = new DateTime(2025, 3, 3),
					Location = location3,
					Status = PalletStatus.Available,
					ProductsOnPallet = new List<ProductOnPallet>
					{
						new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1),DateAdded = new DateTime(2025,4,4) }
					}
				}
			};
			//DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(availablePallets);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var issueRepo = new IssueRepo(DbContext);
			//Act		
			var issue = new Issue
			{
				IssueDateTimeSend = new DateTime(2025, 12, 15),
				Client = initailClient,
				Pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			},
				PerformedBy = "U003"
			};
			issueRepo.AddIssue(issue);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Issues
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == issue.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U003", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q1010");
			Assert.Contains(result.Pallets, p => p.Id == "Q1011");

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
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,				
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.Available,				
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = initialCategory,
				IsDeleted = false,
				CartonsPerPallet = 10,
			};
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);			
			DbContext.Pallets.AddRange(pallet1, pallet2);			
			var issueRepo = new IssueRepo(DbContext);
			//Act		
			var issue = new Issue
			{
				IssueDateTimeSend = new DateTime(2025, 12, 15),
				Client = initailClient,
				Pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			},
				PerformedBy = "U003"
			};
			issueRepo.AddIssue(issue);
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
