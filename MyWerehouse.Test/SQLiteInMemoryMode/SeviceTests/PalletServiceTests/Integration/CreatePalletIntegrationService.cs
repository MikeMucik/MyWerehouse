using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class CreatePalletIntegrationService :TestBase
	{
		[Fact]
		public async Task PalletWithNoHistory_CreatePalletAsync_CreateToList()
		{
			//Arrange
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{

				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var inventory = new Inventory
			{
				Product = product,
				Quantity = 10,
				LastUpdated = DateTime.UtcNow.AddDays(-1)
			};

			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.Inventories.Add(inventory);
			DbContext.SaveChanges();
			var newPallet = new PalletDTO
			{
				ProductsOnPallet = new HashSet<ProductOnPalletDTO>{ new ProductOnPalletDTO
						{
							ProductId = product.Id,
							Quantity = 5,
						}
				},
			};
			//Act
			var result = await Mediator.Send(new CreateNewPalletCommand(newPallet, "user"));
			
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.Contains("do stanu magazynowego, uaktualniono stan magazynowy", result.Message);
			//Assert.False(string.IsNullOrWhiteSpace(result)); // numer palety został zwrócony
			
			// weryfikacja, że paleta faktycznie została utworzona w bazie
			var palletInDb = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync();

			Assert.NotNull(palletInDb);
			

			Assert.Single(palletInDb.ProductsOnPallet);
			Assert.Equal(product.Id, palletInDb.ProductsOnPallet.First().ProductId);
			Assert.Equal(5, palletInDb.ProductsOnPallet.First().Quantity);

			var history = await DbContext.PalletMovements.Include(p => p.PalletMovementDetails)
				.FirstOrDefaultAsync();
			Assert.NotNull(history);
			Assert.NotEmpty(history.PalletMovementDetails);
			Assert.Equal("user", history.PerformedBy);

			var inventoryNew = await DbContext.Inventories
				.FirstOrDefaultAsync(i => i.ProductId == newPallet.ProductsOnPallet.First().ProductId);
			Assert.NotNull(inventoryNew);
			Assert.Equal(15, inventoryNew.Quantity);
		}
		[Fact]
		public async Task PalletWithNoHistory_CreatePalletAsync_ThrowExcpetationValidation()
		{
			//Arrange
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{

				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var inventory = new Inventory
			{
				Product = product,
				Quantity = 10,
				LastUpdated = DateTime.UtcNow.AddDays(-1)
			};

			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.Inventories.Add(inventory);
			DbContext.SaveChanges();
			var newPallet = new PalletDTO
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
			var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
				Mediator.Send(new CreateNewPalletCommand(newPallet, "UserP")));
			//Assert
			Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
		}
		[Fact]
		public async Task PalletWithNoHistory_CreatePalletAsync_ThrowDomainExcpetation()
		{
			//Arrange
			var category = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{

				Aisle = 0,
				Bay = 0,
				Height = 0,
				Position = 0
			};
			var inventory = new Inventory
			{
				Product = product,
				Quantity = 10,
				LastUpdated = DateTime.UtcNow.AddDays(-1)
			};

			DbContext.Locations.Add(location);
			DbContext.Products.Add(product);
			DbContext.Inventories.Add(inventory);
			DbContext.SaveChanges();
			var newPallet = new PalletDTO
			{
				ProductsOnPallet = new HashSet<ProductOnPalletDTO>{
					new ProductOnPalletDTO
						{
							ProductId = 2,
							Quantity = 10,
						}
				},
			};
			//Act
			var result = await Mediator.Send(new CreateNewPalletCommand(newPallet, "user"));
			
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Produkt o numerze 2 nie istnieje.", result.Error);
		}
	}
}
