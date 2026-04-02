using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueMarkAsLoadedServiceTests : TestBase
	{
		[Fact]
		public async Task MarkAsLoaded_ChangeStatus_HappyPath()
		{
			//Arrange
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
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);
			//var product = new Product
			//{
			//	Name = "TestFull",
			//	SKU = "123",
			//	AddedItemAd = new DateTime(2024, 1, 1),
			//	Category = category,
			//	IsDeleted = false,
			//	CartonsPerPallet = 10,
			//};
			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.ToIssue, null, null);
			pallet1.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			//var pallet1 = new Pallet
			//{
			//	PalletNumber = "P1",
			//	DateReceived = new DateTime(2025, 3, 3),
			//	Location = location1,
			//	Status = PalletStatus.ToIssue,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{
			//		new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
			//	}
			//};
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Available, null, null);
			pallet2.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			//var pallet2 = new Pallet
			//{
			//	PalletNumber = "P2",
			//	DateReceived = new DateTime(2025, 3, 3),
			//	Location = location2,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//		{
			//			new ProductOnPallet { Product = product, Quantity = 9, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
			//		}
			//};
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			//var pallet3 = new Pallet
			//{
			//	PalletNumber = "P3",
			//	DateReceived = new DateTime(2025, 3, 3),
			//	Location = location3,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//		{
			//			new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
			//		}
			//};
			//DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand(pallet1.Id, "User123"));
			//Assert
			Assert.True(result.IsSuccess);
			var pallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "P1");
			Assert.NotNull(pallet);
			Assert.Equal(PalletStatus.Loaded, pallet.Status);
		}

		[Fact]
		public async Task MarkAsLoaded_ChangeStatus_SadPath()
		{
			//Arrange
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
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);
			//var product = new Product
			//{
			//	Name = "TestFull",
			//	SKU = "123",
			//	AddedItemAd = new DateTime(2024, 1, 1),
			//	Category = category,
			//	IsDeleted = false,
			//	CartonsPerPallet = 10,
			//};	
			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.ToIssue, null, null);
			pallet1.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			//var pallet1 = new Pallet
			//{
			//	PalletNumber = "P1",
			//	DateReceived = new DateTime(2025, 3, 3),
			//	Location = location1,
			//	Status = PalletStatus.ToIssue,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//	{
			//		new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
			//	}
			//};
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Damaged, null, null);
			pallet2.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			//var pallet2 = new Pallet
			//{
			//	PalletNumber = "P2",
			//	DateReceived = new DateTime(2025, 3, 3),
			//	Location = location2,
			//	Status = PalletStatus.Damaged,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//		{
			//			new ProductOnPallet { Product = product, Quantity = 9, BestBefore = new DateOnly(2026,1,1), DateAdded = new DateTime(2025,4,4) }
			//		}
			//};
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			//var pallet3 = new Pallet
			//{
			//	PalletNumber = "P3",
			//	DateReceived = new DateTime(2025, 3, 3),
			//	Location = location3,
			//	Status = PalletStatus.Available,
			//	ProductsOnPallet = new List<ProductOnPallet>
			//		{
			//			new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
			//		}
			//};
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();
			//Act
			var result = await Mediator.Send(new MarkAsLoadedCommand(pallet2.Id, "User123"));
			//Assert
			Assert.False(result.IsSuccess);
			var pallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "P2");
			Assert.NotNull(pallet);
			Assert.Equal(PalletStatus.Damaged, pallet.Status);
		}
	}
}
