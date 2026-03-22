using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.PickingPallets.Commands.FinishPlannedPickingPrepareToHandPicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class FinishPlannedPickingPrepareToHandPickingIntegrationTests : TestBase
	{
		[Fact]
		public async Task FinishPlannedPickingPrepareToHandPicking_CancelPickingTask_CreateHandPicking()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "777",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location1 = new Location
			{
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
			var sourcePallet1 = new Pallet
			{
				PalletNumber = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var sourcePallet2 = new Pallet
			{
				PalletNumber = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 20,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var sourcePallet3 = new Pallet
			{
				PalletNumber = "Q1002",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 15,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
				IssueDateTimeSend = DateTime.UtcNow.AddHours(-12),
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingTask1 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				ProductId = product1.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			};
			var pickingTask2 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				ProductId = product2.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			};
			var pickingTask3 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 15,
				PickingStatus = PickingStatus.Allocated,
				ProductId = product1.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet2,
				InitialPalletQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask1 }
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			var virtualPallet3 = new VirtualPallet
			{
				Pallet = sourcePallet3,
				InitialPalletQuantity = 15,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask3 }
			};
			pickingTask2.VirtualPallet = virtualPallet2;
			pickingTask3.VirtualPallet = virtualPallet3;
			pickingTask1.VirtualPallet = virtualPallet1;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			await DbContext.SaveChangesAsync();
			//Act 
			var result = Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand());
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Result.Count);
			var resultForProduct1 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product1.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			var resultForProduct2 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product2.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			Assert.NotNull(resultForProduct1);
			Assert.NotNull(resultForProduct2);
			Assert.Equal(25, resultForProduct1.RequestedQuantity);
			Assert.Equal(10, resultForProduct2.RequestedQuantity);
		}
		[Fact]
		public async Task FinishPlannedPickingPartialDonePrepareToHandPicking_CancelPickingTask_CreateHandPicking()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "777",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location1 = new Location
			{
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
			var sourcePallet1 = new Pallet
			{
				PalletNumber = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var sourcePallet2 = new Pallet
			{
				PalletNumber = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 20,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var sourcePallet3 = new Pallet
			{
				PalletNumber = "Q1002",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 15,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
				IssueDateTimeSend = DateTime.UtcNow.AddHours(-12),
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, sourcePallet2, sourcePallet3);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingTask1 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 10,
				PickedQuantity = 5,
				PickingStatus = PickingStatus.PickedPartially,
				ProductId = product1.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			};
			var pickingTask2 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				ProductId = product2.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			};
			var pickingTask3 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 15,
				PickingStatus = PickingStatus.Allocated,
				ProductId = product1.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12))

			};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet2,
				InitialPalletQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask1 }
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask2 }
			};
			var virtualPallet3 = new VirtualPallet
			{
				Pallet = sourcePallet3,
				InitialPalletQuantity = 15,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask3 }
			};
			pickingTask2.VirtualPallet = virtualPallet2;
			pickingTask3.VirtualPallet = virtualPallet3;
			pickingTask1.VirtualPallet = virtualPallet1;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3);
			await DbContext.SaveChangesAsync();
			//Act 
			var result = Mediator.Send(new FinishPlannedPickingPrepareToHandPickingCommand());
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Result.Result.Count);
			var resultForProduct1 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product1.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			var resultForProduct2 = DbContext.PickingTasks.FirstOrDefault(x => x.ProductId == product2.Id && x.IssueId == issue.Id && x.PickingStatus != PickingStatus.Cancelled);
			Assert.NotNull(resultForProduct1);
			Assert.NotNull(resultForProduct2);
			Assert.Equal(20, resultForProduct1.RequestedQuantity);
			Assert.Equal(10, resultForProduct2.RequestedQuantity);
		}
	}
}
