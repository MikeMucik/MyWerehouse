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
using MyWerehouse.Infrastructure.Persistence.Repositories;
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
			{Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);
			
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
			pallet1.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.Now.AddMonths(12)));

			var pallet2 = Pallet.CreateForTests("Q1011", DateTime.Now, 1, PalletStatus.Available, null, null);
			pallet2.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.Now.AddMonths(12)));
					
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2);			
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issueItem =new List<IssueItem>{
				IssueItem.CreateForSeed(1, issueId, product.Id,18, DateOnly.FromDateTime ( DateTime.UtcNow.AddDays(365)), DateTime.UtcNow.AddDays(-7))
			};
			var issue = Issue.CreateForSeed(issueId, 1, client.Id, DateTime.UtcNow.AddDays(-7),
			DateTime.UtcNow.AddDays(7), "UserS", IssueStatus.ConfirmedToLoad, issueItem);
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet2.Id, 20, location1.Id, DateTime.UtcNow.AddDays(-7));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 10, PickingStatus.Picked, product.Id,
				DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)), null, null, 10);
			//virtualPallet.PickingTasks = [pickingTask];
			var pickingPallet = Pallet.CreateForTests("Q5000", DateTime.Now, 1, PalletStatus.ToIssue, null, null);
			pickingPallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			issue.ReservePallet(pallet1);
			issue.ReservePallet(pallet2);
			issue.AttachPickingTask(pickingTask);
			
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.Pallets.Add(pickingPallet);
			DbContext.Issues.Add(issue);
			DbContext.SaveChanges();

			var reversePickingRepo = new ReversePickingRepo(DbContext);
			var reversePicking = ReversePicking.Create(pickingPallet.Id, null, product.Id, pickingPallet.ProductsOnPallet.FirstOrDefault().BestBefore,
				pickingPallet.ProductsOnPallet.FirstOrDefault().Quantity, pickingTask.Id, "UserR");
			//może zmienić na żródło
			//Act
			reversePickingRepo.AddReversePicking(reversePicking);
			DbContext.SaveChanges();
			
			// Assert
			var result = DbContext.ReversePickings
				.Include(rp => rp.PickingTask)
				.SingleOrDefault();

			Assert.NotNull(result);

			// --- klucze i wymagane pola ---
			Assert.True(result.Id != null);

			Assert.Equal(pickingPallet.Id, result.PickingPalletId);
			Assert.Equal(pickingTask.Id, result.PickingTaskId);
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
			Assert.NotNull(result.PickingTask);
			Assert.Equal(pickingTask.Id, result.PickingTask.Id);

			// ilość reverse picking nie może przekraczać alokacji
			Assert.True(result.Quantity <= pickingTask.RequestedQuantity);

			// BestBefore musi dotyczyć tego samego produktu
			Assert.Equal(
				pickingTask.BestBefore,
				result.BestBefore
			);

		}
	}
}
