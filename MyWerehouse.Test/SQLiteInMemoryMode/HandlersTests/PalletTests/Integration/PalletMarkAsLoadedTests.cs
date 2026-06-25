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

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletTests.Integration
{
	public class PalletMarkAsLoadedTests : TestBase
	{
		private Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, 1, 56);
		}
		private Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "TestC",
				IsDeleted = false
			};
		}
		private Location CreateLocation(int position)
		{
			return new Location
			{
				Bay = 0,
				Aisle = 0,
				Height = 0,
				Position = position
			};
		}
		[Fact]
		public async Task MarkAsLoaded_ShouldChangeStatus_WhenStatusPalletToIssue()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct("Test", "666666");
			var location = CreateLocation(0);			
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
			Assert.Contains("Paleta Q0001 załadowana.", result.Message);
			Assert.Equal(PalletStatus.Loaded, result.Result.NewStatus);
			var history = DbContext.HistoryPallet.FirstOrDefault(p => p.PalletId == pallet.Id);
			Assert.NotNull(history);
		}
		[Fact]
		public async Task MarkAsLoaded_ShouldChangeStatus_WhenStatusIsLockedForIssueAfterReplacement() // when pallet was changed
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct("Test", "666666");
			var location = CreateLocation(0);			
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
			Assert.Contains("Paleta Q0001 załadowana.", result.Message);
			Assert.Equal(PalletStatus.Loaded, result.Result.NewStatus);
		}
		[Fact]
		public async Task MarkAsLoaded_ShouldReturnInfo_WhenWrongPalletStatus()
		{
			//Arrange

			var category = CreateCategory();
			var product = CreateProduct("Test", "666666");
			var location = CreateLocation(0);
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
