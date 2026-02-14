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
				Name = "Category",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
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
			var sourcePallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8),
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))
					}
				}
			};
			var pickingPallet = new Pallet
			{
				Id = "Q1001",
				Location = locationPicking,
				Status = PalletStatus.Picking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 10,
						DateAdded = DateTime.UtcNow.AddDays(-1)
					}
				}
			};
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow.AddDays(-6),
				IssueDateTimeSend = DateTime.UtcNow.AddDays(1),
				IssueStatus = IssueStatus.Pending,
				PerformedBy = "TestUser",
				Pallets = []
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet, pickingPallet);
			DbContext.Issues.AddRange(issue);
			DbContext.SaveChanges();
			var virtualPallet = new VirtualPallet
			{
				Pallet = sourcePallet,
				InitialPalletQuantity = 40,
				Location = sourcePallet.Location,
				DateMoved = DateTime.UtcNow.AddDays(-8),
			};
			var pickingTask = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 30,
				PickingStatus = PickingStatus.Picked,
				VirtualPallet = virtualPallet,
				PickedQuantity = 30,
				PickingPalletId = pickingPallet.Id,
				PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
				ProductId = product.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365))

			};
			virtualPallet.PickingTasks = [pickingTask];
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
