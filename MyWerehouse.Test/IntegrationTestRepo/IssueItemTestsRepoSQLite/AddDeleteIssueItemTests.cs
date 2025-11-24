using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.IssueItemTestsRepoSQLite
{
	public class AddDeleteIssueItemTests : TestBase
	{
		[Fact]
		public void AddItem_AddIssueItem_AddToCollection()
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
			var issue = new Issue
			{
				IssueDateTimeSend = new DateTime(2025, 12, 15),
				Client = initailClient,
				Pallets = new List<Pallet>(),
				PerformedBy = "U003"
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Clients.Add(initailClient);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();
			var item = new CreateIssueDTO{
				ClientId = initailClient.Id,
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO
					{
						ProductId = product.Id,
						Quantity = 10,
						IssueId = issue.Id,
						BestBefore = new DateOnly(2026,1,1)
					}
				}
			};			
			var newItem = new IssueItem
			{
				Issue = issue,
				ProductId = item.Items.First().ProductId,
				Quantity = item.Items.First().Quantity,
				BestBefore = item.Items.First().BestBefore,
			};
			var issueItemRepo = new IssueItemRepo(DbContext);
			//Act
			issueItemRepo.AddIssueItem(newItem);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.IssueItems.Find(newItem.Id);
			Assert.NotNull(result);
		}

	}
}
