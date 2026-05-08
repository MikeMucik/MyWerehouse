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
			{	Id = 1,
				Name = "Category",
				IsDeleted = false
			};
			var product = Product.Create("Prod B", "777", 1, 100);
			
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow.AddDays(-5),
			DateTime.UtcNow.AddDays(1), "TestUser", IssueStatus.Pending, null);
			var sourcePallet = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToPicking, null, null);
			sourcePallet.AddProductForTests(product.Id, 60, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pickingPallet = Pallet.CreateForTests("Q1001", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			pickingPallet.AddProductForTests(product.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.Now.AddMonths(24)));
			
			DbContext.Pallets.AddRange(sourcePallet, pickingPallet);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();
			
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), sourcePallet.Id, 100, sourcePallet.Location.Id, new DateTime(2025, 8, 12));
			
			var pickinTaskGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickinTaskGuid, virtualPallet.Id, issueId, 40,
				PickingStatus.Picked, product.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)),
				pickingPallet.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 40);
			DbContext.PickingTasks.Add(pickingTask);
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
			var product = Product.Create("Prod B", "777", 1, 100);
			
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
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var issueId = Guid.NewGuid();
			var issue = Issue.CreateForSeed(issueId, 1, 1, DateTime.UtcNow.AddDays(-5),
			DateTime.UtcNow.AddDays(1), "TestUser", IssueStatus.Pending, null);
			var sourcePallet1 = Pallet.CreateForTests("Q1000", new DateTime(2025, 8, 8), 1, PalletStatus.ToIssue, null, issueId);
			sourcePallet1.AddProductForTests(product.Id, 100, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.Now.AddMonths(24)));
			
			DbContext.Pallets.AddRange(sourcePallet1);
			DbContext.Issues.AddRange(issue);
			await DbContext.SaveChangesAsync();

			//Act & Assert
			var result = await _createReversePickingTask.CreateReversePicking(sourcePallet1.Id, "UserReverse");
			DbContext.SaveChanges();
			//Assert.Contains("Brak alokacji dla palety. Paleta nie do dekompletacji.", result.Er);
			Assert.Contains("Brak alokacji dla palety. Paleta nie do dekompletacji.", result.Message);
		}
	}
}
