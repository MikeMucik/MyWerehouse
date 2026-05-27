using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletServiceTests.Integration
{
	public class PalletMarkAsLoadedTests : TestBase
	{
		[Fact]
		public async Task MarkAsLoaded_ShouldChangeStatus_WhenStatusPalletToIssue()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("Test", "666666", 1, 56);
			var location = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var pallet = Pallet.CreateForTests("Q0001", DateTime.UtcNow.AddDays(-30), location.Id, PalletStatus.ToIssue, null, null);
			pallet.AddProductForTests(product.Id, 10, DateTime.UtcNow.AddDays(-30), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand(pallet.Id, "User"));
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Paleta załadowana.", result.Message);
			Assert.Equal(PalletStatus.Loaded, result.Result.NewStatus);
			var history = DbContext.HistoryPallet.FirstOrDefault(p => p.PalletId == pallet.Id);
			Assert.NotNull(history);
		}
		[Fact]
		public async Task MarkAsLoaded_ShouldChangeStatus_WhenStatusPalletLockedForIssue()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("Test", "666666", 1, 56);
			var location = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var pallet = Pallet.CreateForTests("Q0001", DateTime.UtcNow.AddDays(-30), location.Id, PalletStatus.LockedForIssue, null, null);
			pallet.AddProductForTests(product.Id, 10, DateTime.UtcNow.AddDays(-30), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand(pallet.Id, "User"));
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Paleta załadowana.", result.Message);
			Assert.Equal(PalletStatus.Loaded, result.Result.NewStatus);
		}
		[Fact]
		public async Task MarkAsLoaded_ShouldReturnInfo_WhenWrongPalletStatus()
		{
			//Arrange
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("Test", "666666", 1, 56);
			var location = new Location
			{
				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var pallet = Pallet.CreateForTests("Q0001", DateTime.UtcNow.AddDays(-30), location.Id, PalletStatus.Archived, null, null);
			pallet.AddProductForTests(product.Id, 10, DateTime.UtcNow.AddDays(-30), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand(pallet.Id, "User"));
			//Assert
			Assert.NotNull(result);
			Assert.Contains("Paleta nie ma statusu do załadowania", result.Error);
		}
	}
}
