using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.ReversePickingServiceTests.Unit
{
	public class CreateTaskToReversePickingTests : TestBase
	{	
		//HappyPath
		[Fact]
		public async Task CreateTaskToReversePicking_ProperData_AddToBase()
		{
			//Arrange
			var _palletRepo = new PalletRepo(DbContext);
			var _pickingTaskRepo = new PickingTaskRepo(DbContext);
			var _reversePickingRepo = new ReversePickingRepo(DbContext);
			var _createReversePickingTask = new CreateReversePickingService(_palletRepo, _pickingTaskRepo, _reversePickingRepo);
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
			var sourcePallet = new Pallet
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
						Quantity = 60,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			}; 
			var pickingPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location,
				Status = PalletStatus.ToIssue,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow.AddDays(-5),				
				IssueStatus = IssueStatus.Pending,
				PerformedBy = "TestUser",
				IssueDateTimeSend = DateTime.UtcNow.AddDays(1),
				Pallets = [pickingPallet]
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.Pallets.AddRange(sourcePallet, pickingPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			var pickingTask = new PickingTask
			{
				Issue = issue,
				RequestedQuantity = 40,
				PickingStatus = PickingStatus.Picked,
				ProductId = product.Id,
				PickedQuantity = 40,
				PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
				PickingPalletId = pickingPallet.Id,
			};
			var virtualPallet = new VirtualPallet
			{
				Pallet = sourcePallet,
				InitialPalletQuantity = 100,
				Location = sourcePallet.Location,
				DateMoved = new DateTime(2025, 8, 12),
				PickingTasks = new List<PickingTask> { pickingTask }
			};
			pickingTask.VirtualPallet = virtualPallet;

			DbContext.VirtualPallets.AddRange(virtualPallet);
			DbContext.SaveChanges();
			//Act
			
			await _createReversePickingTask.CreateReversePicking(pickingPallet.Id, "UserReverse");
			DbContext.SaveChanges();
			//Assert			
			var taskReverse = DbContext.ReversePickings.SingleOrDefault();
			Assert.NotNull(taskReverse);

			// 1. Poprawna paleta źródłowa
			Assert.Equal(pickingPallet.Id, taskReverse.PickingPalletId);

			// 2. Poprawne Issue
			Assert.Equal(issue.Id, taskReverse.PickingTask.IssueId);

			// 3. Ilość cofnięta
			Assert.Equal(40, taskReverse.Quantity);

			// 4. Użytkownik wykonujący
			Assert.Equal("UserReverse", taskReverse.UserId);

			// 5. Reverse powinien być zapisany w bazie tylko jeden
			Assert.Equal(1, DbContext.ReversePickings.Count());
			}
		//SadPath
		[Fact]
		public async Task CreateTaskToReversePicking_NonPickingPallet_ThrowInfo()
		{
			//Arrange
			var _palletRepo = new PalletRepo(DbContext);
			var _pickingTaskRepo = new PickingTaskRepo(DbContext);
			var _reversePickingRepo = new ReversePickingRepo(DbContext);
			var _createReversePickingTask = new CreateReversePickingService(_palletRepo, _pickingTaskRepo, _reversePickingRepo);
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
				Id = Guid.NewGuid(),
				IssueNumber = 1,
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

			//Act & Assert
			var result = await _createReversePickingTask.CreateReversePicking(sourcePallet1.Id, "UserReverse");
			//var ex = await Assert.ThrowsAsync<NotFoundPickingTaskException>(() => _createReversePickingTask.CreateReversePicking(sourcePallet1.Id, "UserReverse"));
			DbContext.SaveChanges();
			Assert.Contains("Brak alokacji dla palety. Paleta nie do dekompletacji.", result.Message);
		}
	}
}
