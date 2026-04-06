using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class ClosePalletPickingIntegrationTests :TestBase
	{
		[Fact]
		public async Task ClosePalletPicking_ProperPallet_ChangeStatus()
		{
			var category = new Category
			{
				Id =1,
				Name = "Category",
				IsDeleted = false
			};
			var product = Product.Create("Prod A", "666", 1, 100);
			
			var location1 = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pickingPallet = Pallet.CreateForTests("Q1001", DateTime.Now, 100100, PalletStatus.Picking, null, null);
			pickingPallet.AddProductForTests(product.Id, 10, DateTime.UtcNow, DateOnly.FromDateTime(DateTime.Now.AddMonths(24)));
			
			var issueId = Guid.NewGuid();
			
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.Now.AddDays(-6),
			DateTime.Now.AddDays(1), "user1", IssueStatus.Pending, null);
		
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet, pickingPallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 40, sourcePallet.LocationId, DateTime.UtcNow.AddDays(-8));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, virtualPallet.Id, issue.Id, 30, PickingStatus.Picked, product.Id,
				 DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)), pickingPallet.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 30);
			//virtualPallet.PickingTasks = [pickingTask];
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new ClosePickingPalletCommand(pickingPallet.Id, issue.Id, "UserPicker"));
			//Assert
			Assert.NotNull(result);
			var closedPallet = DbContext.Pallets.SingleOrDefault(p => p.Id == pickingPallet.Id);
			Assert.NotNull(closedPallet);
			Assert.Equal(PalletStatus.ToIssue, closedPallet.Status);
			Assert.Equal(issue.Id, closedPallet.IssueId);
			var history = DbContext.PalletMovements.SingleOrDefault(p => p.PalletId == pickingPallet.Id);
			Assert.NotNull(history);
			Assert.Equal(ReasonMovement.ToLoad, history.Reason);
			Assert.Contains("Zamknięto paletę, dołączono do zlecenia", result.Message);
		}
	}
}
