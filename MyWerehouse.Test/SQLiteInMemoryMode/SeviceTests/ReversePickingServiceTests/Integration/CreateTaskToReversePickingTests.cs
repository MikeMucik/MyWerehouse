using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.ReversePickingServiceTests.Integration
{
	public class CreateTaskToReversePickingTests : ReverseIntegrationCommandService
	{
		//HappyPath
		[Fact]
		public async Task CreateTaskToReversePicking_ProperData_AddToBase()
		{
			//Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Prod B",
				SKU = "777",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location = new Location
			{
				Aisle = 1,
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
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 100,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
				IssueDateTimeSend = DateTime.UtcNow,
				Pallets = [sourcePallet1]
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.Pallets.AddRange(sourcePallet1);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingTask1 = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 5,
				PickingStatus = PickingStatus.Picked,
				ProductId = product.Id,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
				PickingPalletId = sourcePallet1.Id,
			};
			var virtualPallet = new VirtualPallet
			{
				Pallet = sourcePallet1,
				InitialPalletQuantity = 100,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask1 }
			};
			pickingTask1.VirtualPallet = virtualPallet;

			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.SaveChanges();
			//Act
			//var result =
				await _reversePickingService.CreateTaskToReversePickingAsync(sourcePallet1.Id, "UserReverse");
			//Assert
			var taskReverse = DbContext.ReversePickings.FirstOrDefault();
			Assert.NotNull(taskReverse);
			//Assert.NotNull(result);
			//Assert.True(result.First().Success);
		}
		//SadPath
		//[Fact]
		//public async Task CreateTaskToReversePicking_NonPickingPallet_ThrowInfo()
		//{
		//	//Arrange
		//	var category = new Category
		//	{
		//		Name = "Category",
		//		IsDeleted = false
		//	};
		//	var product = new Product
		//	{
		//		Name = "Prod B",
		//		SKU = "777",
		//		AddedItemAd = new DateTime(2025, 1, 1),
		//		Category = category,
		//		IsDeleted = false,
		//		CartonsPerPallet = 100
		//	};
		//	var location = new Location
		//	{
		//		Aisle = 1,
		//		Bay = 1,
		//		Height = 1,
		//		Position = 1
		//	};
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var client = new Client
		//	{
		//		Name = "Client A",
		//		Email = "123@wp.pl",
		//		Description = "des",
		//		FullName = "full",
		//		Addresses = [address],
		//		IsDeleted = false,
		//	};
		//	var sourcePallet1 = new Pallet
		//	{
		//		Id = "Q1000",
		//		DateReceived = new DateTime(2025, 8, 8),
		//		Location = location,
		//		Status = PalletStatus.ToPicking,
		//		ProductsOnPallet = new List<ProductOnPallet>
		//		{
		//			new ProductOnPallet
		//			{
		//				Product = product,
		//				Quantity = 100,
		//				DateAdded = new DateTime(2025, 8, 8) }
		//		}
		//	};
		//	var issue = new Issue
		//	{
		//		Client = client,
		//		IssueDateTimeCreate = DateTime.UtcNow,
		//		IssueStatus = IssueStatus.New,
		//		PerformedBy = "TestUser",
		//		IssueDateTimeSend = DateTime.UtcNow,
		//		Pallets = [sourcePallet1]
		//	};
		//	DbContext.Addresses.Add(address);
		//	DbContext.Categories.Add(category);
		//	DbContext.Locations.Add(location);
		//	DbContext.Clients.Add(client);
		//	DbContext.Products.Add(product);
		//	DbContext.Pallets.AddRange(sourcePallet1);
		//	DbContext.Issues.AddRange(issue);
		//	await DbContext.SaveChangesAsync();
			
		//	//Act & Assert
		//	var ex =await Assert.ThrowsAsync < NotFoundAlloactionException >(() => _reversePickingService.CreateTaskToReversePickingAsync(sourcePallet1.Id, "UserReverse"));
			
		//	Assert.Contains("Brak alokacji dla palety. Paleta nie do dekompletacji.", ex.Message);
		//}
	}
}
