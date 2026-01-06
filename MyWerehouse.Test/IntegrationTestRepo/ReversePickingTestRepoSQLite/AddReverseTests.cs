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
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ReversePickingTestRepoSQLite
{
	public class AddReverseTests : TestBase
	{
		[Fact]
		public void AddReversePicking_AddTask_AddToCollection()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "TestFull",
				SKU = "123",
				AddedItemAd = new DateTime(2024, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 10,
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
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet= [new ProductOnPallet {
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddMonths(-1),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
					}]
				//ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				Location = location2,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet {
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddMonths(-1),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
					}]
				//ReceiptId = 10,
			};			
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2);			
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow.AddDays(-7),
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				//Pallets = [pallet1, pickingPallet],
				PerformedBy = "UserS",
				//Allocations = [allocation],
				IssueStatus = IssueStatus.ConfirmedToLoad,
				IssueItems = [new IssueItem {
					CreatedAt = DateTime.UtcNow.AddDays(-7),
					Product = product,
					Quantity = 18,
					BestBefore = DateOnly.FromDateTime ( DateTime.UtcNow.AddDays(365))
				}]
			};
			var virtualPallet = new VirtualPallet
			{
				PalletId = pallet2.Id,
				IssueInitialQuantity = 20,
				LocationId = location1.Id,
				DateMoved = DateTime.UtcNow.AddDays(-7),			
			};
			var allocation = new Allocation
			{
				Issue = issue,
				PickingStatus = PickingStatus.Picked,
				Quantity = 10,
				VirtualPallet = virtualPallet
			};

			virtualPallet.Allocations = [allocation];

			var pickingPallet = new Pallet
			{
				Id = "Q5000",
				LocationId = location1.Id,
				Status = PalletStatus.ToIssue,
				DateReceived = DateTime.UtcNow,
				IssueId = 1,
				ProductsOnPallet = [new ProductOnPallet {
					ProductId = product.Id,
					Quantity = 10,
					DateAdded = DateTime.UtcNow.AddDays(-7),
					BestBefore =DateOnly.FromDateTime( DateTime.UtcNow.AddDays(365))
				}]
			};
			issue.Pallets = [pallet1, pickingPallet];
			issue.Allocations = [allocation];
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.Allocations.Add(allocation);
			DbContext.Pallets.Add(pickingPallet);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();

			var reversePickingRepo = new ReversePickingRepo(DbContext);
			var reversePicking = new ReversePicking
			{
				PickingPalletId = pickingPallet.Id,
				AllocationId = allocation.Id,
				ProductId = product.Id,
				BestBefore = pickingPallet.ProductsOnPallet.FirstOrDefault().BestBefore,
				Quantity = pickingPallet.ProductsOnPallet.FirstOrDefault().Quantity,
				UserId = "UserR"
			};
			//Act
			reversePickingRepo.AddReversePicking(reversePicking);
			DbContext.SaveChanges();
			
			// Assert
			var result = DbContext.ReversePickings
				.Include(rp => rp.Allocation)
				.SingleOrDefault();

			Assert.NotNull(result);

			// --- klucze i wymagane pola ---
			Assert.True(result.Id > 0);

			Assert.Equal(pickingPallet.Id, result.PickingPalletId);
			Assert.Equal(allocation.Id, result.AllocationId);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal("UserR", result.UserId);

			// --- dane ilościowe i daty ---
			Assert.Equal(10, result.Quantity);
			Assert.Equal(
				pickingPallet.ProductsOnPallet.First().BestBefore,
				result.BestBefore
			);

			// --- status ReversePicking ---
			Assert.Equal(ReversePickingStatus.Pending, result.Status);

			// --- palety źródłowe / docelowe ---
			Assert.Null(result.SourcePalletId);
			Assert.Null(result.DestinationPalletId);

			// --- relacja ---
			Assert.NotNull(result.Allocation);
			Assert.Equal(allocation.Id, result.Allocation.Id);

			// ilość reverse picking nie może przekraczać alokacji
			Assert.True(result.Quantity <= allocation.Quantity);

			// BestBefore musi dotyczyć tego samego produktu
			Assert.Equal(
				allocation.VirtualPallet.Pallet.ProductsOnPallet.First().BestBefore,
				result.BestBefore
			);

		}
	}
}
