using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletTests.Integration
{
	public class CreatePalletIntegrationServiceTests :TestBase
	{
		private static readonly Guid guid = Guid.NewGuid();
		private readonly Guid productId = guid;
		private static Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name)
		{
			return Product.CreateForSeed(productId, name, "666666", DateTime.UtcNow, 1,false, 56);
		}
		private static Location CreateLocation(int position)
		{
			return new Location
			{
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		private Inventory CreateInventory()
		{
			return new Inventory
			{
				ProductId = productId,
				Quantity = 10,
				LastUpdated = DateTime.UtcNow.AddDays(-1)
			};
		}
		[Fact]
		public async Task CreatePallet_ShouldCreate_WhenValidData()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct("Test");
			var location = CreateLocation(0);
			var inventory = CreateInventory();
			
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.Inventories.Add(inventory);
			DbContext.SaveChanges();
			var newPallet = new CreatePalletDTO
			{
				ProductsOnPallet = new HashSet<ProductOnPalletDTO>{ new ProductOnPalletDTO
						{
							ProductId = product.Id,
							Quantity = 5,
						}
				},
			};
			//Act
			var result = await Mediator.Send(new CreatePalletCommand(newPallet,1, "user"));
			
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("do stanu magazynowego, uaktualniono stan magazynowy", result.Message);
			
			// weryfikacja, że paleta faktycznie została utworzona w bazie
			var palletInDb = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync();

			Assert.NotNull(palletInDb);			

			Assert.Single(palletInDb.ProductsOnPallet);
			Assert.Equal(product.Id, palletInDb.ProductsOnPallet.First().ProductId);
			Assert.Equal(5, palletInDb.ProductsOnPallet.First().Quantity);

			var history = await DbContext.HistoryPallet.Include(p => p.HistoryPalletDetails)
				.FirstOrDefaultAsync();
			Assert.NotNull(history);
			Assert.NotEmpty(history.HistoryPalletDetails);
			Assert.Equal("user", history.PerformedBy);

			var inventoryNew = await DbContext.Inventories
				.FirstOrDefaultAsync(i => i.ProductId == newPallet.ProductsOnPallet.First().ProductId);
			Assert.NotNull(inventoryNew);
			Assert.Equal(15, inventoryNew.Quantity);
		}
		[Fact]
		public async Task CreatePallet_ThrowExcpetationValidation_WhenQuantityIsZero()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct("Test");
			var location = CreateLocation(0);
			var inventory = CreateInventory();			
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.Inventories.Add(inventory);
			DbContext.SaveChanges();
			var newPallet = new CreatePalletDTO
			{
				ProductsOnPallet = new HashSet<ProductOnPalletDTO>{
					new ProductOnPalletDTO
						{
							ProductId = product.Id,
							Quantity = 0,
						}
				},
			};
			//Act
			var ex = await Assert.ThrowsAsync<ValidationException>(() =>
				Mediator.Send(new CreatePalletCommand(newPallet,1, "UserP")));
			//Assert
			Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
		}
		[Fact]
		public async Task CreatePallet_ThrowExcptionValidation_WhenProductNotExist()
		{
			//Arrange
			var category = CreateCategory();
			var product = CreateProduct("Test");
			var location = CreateLocation(0);
			var inventory = CreateInventory();
			
			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.Inventories.Add(inventory);
			DbContext.SaveChanges();
			var product9Id = Guid.Parse("00000000-0000-0000-0009-000000000000");
			var newPallet = new CreatePalletDTO
			{
				ProductsOnPallet = new HashSet<ProductOnPalletDTO>{
					new ProductOnPalletDTO
						{
							ProductId =product9Id,
							Quantity = 10,
						}
				},
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(()=> Mediator.Send(new CreatePalletCommand(newPallet, 1, "user")));
			Assert.Contains("Wybrany product nie istnieje.", ex.Message);			
		}
	}
}
