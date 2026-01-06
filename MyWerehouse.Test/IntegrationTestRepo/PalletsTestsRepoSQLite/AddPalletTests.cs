using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;


namespace MyWerehouse.Test.IntegrationTestRepo.PalletsTestsRepoSQLite
{
	public class AddPalletTests : TestBase
	{
		[Fact]
		public void AddEmptyPallet_AddPallet_AddToCollection()
		{
			//Arrange
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var palletRepo = new PalletRepo(DbContext);
			//Act
			var result = palletRepo.AddPallet(pallet);
			DbContext.SaveChanges();
			//Assert			
			var createdPallet = DbContext.Pallets.Find(pallet.Id);
			Assert.NotNull(createdPallet);
			Assert.Equal(pallet.Status, createdPallet.Status);
		}
		[Fact]
		public void AddPalletWithProduct_AddPallet_AddToCollection()
		{
			//Arrange
			var category = new Category
			{
				Name = "TestC",
			};
			var product = new Product
			{
				Category = category,
				Name = "TestP",
				SKU = "1234Test",
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
				ProductsOnPallet = new List<ProductOnPallet> {
						new ProductOnPallet
						{
							Product = product,
							Quantity = 1,
							DateAdded = DateTime.Now,
							BestBefore = DateOnly.FromDateTime(DateTime.Now.AddMonths(24)),
						}
				}
			};
			var palletRepo = new PalletRepo(DbContext);
			//Act
			var result = palletRepo.AddPallet(pallet);
			DbContext.SaveChanges();
			//Assert			
			
			var createdPallet = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.ThenInclude(pp => pp.Product)
				.FirstOrDefault(p => p.Id == pallet.Id);

			Assert.NotNull(createdPallet); // paleta została dodana
			Assert.Equal(pallet.Id, createdPallet.Id); // identyfikator się zgadza
			Assert.Equal(pallet.Status, createdPallet.Status); // status ten sam
			Assert.Equal(pallet.LocationId, createdPallet.LocationId); // przypisana lokalizacja poprawna
			Assert.Equal(location.Id, createdPallet.LocationId); // zgodność z utworzoną lokalizacją

			// sprawdzenie relacji z produktem
			Assert.NotNull(createdPallet.ProductsOnPallet);
			Assert.Single(createdPallet.ProductsOnPallet); // dokładnie jeden produkt na palecie

			var productOnPallet = createdPallet.ProductsOnPallet.First();
			Assert.Equal(product.Id, productOnPallet.ProductId);
			Assert.Equal(1, productOnPallet.Quantity);
			Assert.Equal(product.Name, productOnPallet.Product.Name);
			Assert.Equal("1234Test", productOnPallet.Product.SKU);
			Assert.Equal(category.Id, productOnPallet.Product.CategoryId);
			Assert.Equal("TestC", productOnPallet.Product.Category.Name);

			// dodatkowa kontrola poprawnego zapisu w kontekście
			Assert.True(DbContext.Pallets.Any(p => p.Id == pallet.Id));
			Assert.True(DbContext.ProductOnPallet.Any(pop => pop.PalletId == pallet.Id));
		}
		[Fact]
		public void AddPalletWithTwoProducts_AddPallet_AddToCollection()
		{
			//Arrange
			var category = new Category
			{
				Name = "TestC",
			};
			var product1 = new Product
			{
				Category = category,
				Name = "TestP",
				SKU = "1234Test",
			};
			var product2 = new Product
			{
				Category = category,
				Name = "TestP2",
				SKU = "1234Test2",
			};
			var location = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.Add(location);
			DbContext.SaveChanges();
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
				ProductsOnPallet = new List<ProductOnPallet> {
						new ProductOnPallet
						{
							Product = product1,
							Quantity = 1,
							DateAdded = DateTime.Now,
							BestBefore = DateOnly.FromDateTime(DateTime.Now.AddMonths(24)),
						},
						new ProductOnPallet
						{
							Product = product2,
							Quantity = 20,
							DateAdded = DateTime.Now,
							BestBefore = DateOnly.FromDateTime(DateTime.Now.AddMonths(24)),
						}
				}
			};
			var palletRepo = new PalletRepo(DbContext);
			//Act
			var result = palletRepo.AddPallet(pallet);
			DbContext.SaveChanges();
			//Assert			

			var createdPallet = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.ThenInclude(pp => pp.Product)
				.FirstOrDefault(p => p.Id == pallet.Id);

			Assert.NotNull(createdPallet); // paleta została dodana
			Assert.Equal(pallet.Id, createdPallet.Id); // identyfikator się zgadza
			Assert.Equal(pallet.Status, createdPallet.Status); // status ten sam
			Assert.Equal(pallet.LocationId, createdPallet.LocationId); // przypisana lokalizacja poprawna
			Assert.Equal(location.Id, createdPallet.LocationId); // zgodność z utworzoną lokalizacją

			// sprawdzenie relacji z produktem
			Assert.NotNull(createdPallet.ProductsOnPallet);
			Assert.Equal(2,createdPallet.ProductsOnPallet.Count); 

			var productOnPallet1 = createdPallet.ProductsOnPallet.First(x=>x.Product==product1);
			Assert.Equal(product1.Id, productOnPallet1.ProductId);
			Assert.Equal(1, productOnPallet1.Quantity);
			Assert.Equal(product1.Name, productOnPallet1.Product.Name);
			Assert.Equal("1234Test", productOnPallet1.Product.SKU);
			Assert.Equal(category.Id, productOnPallet1.Product.CategoryId);
			Assert.Equal("TestC", productOnPallet1.Product.Category.Name);

			var productOnPallet2 = createdPallet.ProductsOnPallet.First(x => x.Product == product2);
			Assert.Equal(product2.Id, productOnPallet2.ProductId);
			Assert.Equal(20, productOnPallet2.Quantity);
			Assert.Equal(product2.Name, productOnPallet2.Product.Name);
			Assert.Equal("1234Test2", productOnPallet2.Product.SKU);
			Assert.Equal(category.Id, productOnPallet2.Product.CategoryId);
			Assert.Equal("TestC", productOnPallet2.Product.Category.Name);
			// dodatkowa kontrola poprawnego zapisu w kontekście
			Assert.True(DbContext.Pallets.Any(p => p.Id == pallet.Id));
			Assert.True(DbContext.ProductOnPallet.Any(pop => pop.PalletId == pallet.Id));
		}
	}
}
