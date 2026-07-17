using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.VirtualPalletTestsRepoSQLite
{
	public class AddETCVirtualPalletTests : TestBase
	{
		[Fact]
		public void AddNewRecord_AddPalletToPicking_AddToCollectionVirtualPallet()
		{
			//Arrange
			var newCategory = new Category
			{
				Id = 1,
				Name = "CategoryName"
			};
			var product = Product.Create("Banana", "1234567890", TestDates.UtcNow, 1, 56, 30, 30, 30, 30, "TestDetails");
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Categories.Add(newCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var pallet = Pallet.CreateForTests("Q00001", TestDates.Now, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(12)));
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			var virtualPallet = VirtualPallet.Create(pallet.Id, pallet.ProductsOnPallet.First().Quantity, pallet.LocationId, TestDates.UtcNow);
			DbContext.SaveChanges();
			var virtualPalletRepo = new VirtualPalletRepo(DbContext);
			//Act
			virtualPalletRepo.AddPalletToPicking(virtualPallet);
			DbContext.SaveChanges();
			//Assert
			var createdVirtualPallet = DbContext.VirtualPallets
				.Include(v => v.Pallet)
				.ThenInclude(p => p.ProductsOnPallet)
				.FirstOrDefault(v => v.Id == virtualPallet.Id);

			Assert.NotNull(createdVirtualPallet);
			Assert.Equal(virtualPallet.Id, createdVirtualPallet.Id);
			// Sprawdź relację z Pallet
			Assert.NotNull(createdVirtualPallet.Pallet);
			Assert.Equal("Q00001", createdVirtualPallet.Pallet.PalletNumber);
			Assert.Equal(pallet.LocationId, createdVirtualPallet.LocationId);
			// Sprawdź ilości
			Assert.Equal(10, createdVirtualPallet.InitialPalletQuantity);
			Assert.Empty(createdVirtualPallet.PickingTasks);
			// Sprawdź powiązany produkt
			var productOnPallet = createdVirtualPallet.Pallet.ProductsOnPallet.FirstOrDefault();
			Assert.NotNull(productOnPallet);
			Assert.Equal("Banana", productOnPallet.Product.Name);
			Assert.Equal("1234567890", productOnPallet.Product.SKU);
			Assert.Equal(10, productOnPallet.Quantity);
			// Sprawdź, że VirtualPallet faktycznie trafił do kolekcji VirtualPallets w DbContext
			Assert.Contains(DbContext.VirtualPallets, v => v.Id == virtualPallet.Id);
		}
		[Fact]
		public void DeleteRecord_DeletePalletToPicking_RemoveFromCollectionVirtualPallet()
		{
			//Arrange
			var newCategory = new Category
			{
				Name = "CategoryName"
			};
			DbContext.Categories.Add(newCategory);

			var product = Product.Create("Banana", "1234567890", TestDates.UtcNow, 1, 56, 30, 30, 30, 30, "TestDetails");
			DbContext.Products.Add(product);
			DbContext.SaveChanges();
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Locations.Add(location);
			var pallet = Pallet.CreateForTests("Q00001", TestDates.Now, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddMonths(12)));

			DbContext.Pallets.Add(pallet);
			var virtualPallet = VirtualPallet.Create(pallet.Id, pallet.ProductsOnPallet.First().Quantity, pallet.LocationId, TestDates.UtcNow);
			DbContext.VirtualPallets.Add(virtualPallet);
			DbContext.SaveChanges();
			var virtualPalletRepo = new VirtualPalletRepo(DbContext);
			//Act
			virtualPalletRepo.DeleteVirtualPalletPicking(virtualPallet);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.VirtualPallets.Find(virtualPallet.Id);
			Assert.Null(result);
		}		
	}
}
