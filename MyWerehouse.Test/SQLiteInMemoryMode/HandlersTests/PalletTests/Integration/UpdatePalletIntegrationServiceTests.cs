using System;
using System.Collections.Generic;
using FluentValidation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletTests.Integration
{

	public class UpdatePalletIntegrationServiceTests : TestBase
	{
		Guid productId = Guid.NewGuid();
		Guid productId1 = Guid.NewGuid();
		Guid productId2 = Guid.NewGuid();
		Guid productId3 = Guid.NewGuid();
		private Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
		}
		private Product CreateProduct(Guid id, string name, string sku)
		{
			return Product.CreateForSeed(id, name, sku, DateTime.UtcNow, 1, false, 56);
		}
		private Location CreateLocation(int position)
		{
			return new Location
			{
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		private Inventory CreateInventory(Guid id, int quantity)
		{
			return new Inventory
			{
				ProductId = id,
				Quantity = quantity,
				LastUpdated = DateTime.UtcNow.AddDays(-1)
			};
		}
		[Fact]
		public async Task UpdatePallet_ShouldIncreasingQuantity_WhenProperData()
		{
			//Arange	
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test11", "67777");
			var product2 = CreateProduct(productId2, "Test22", "667777");
			var product3 = CreateProduct(productId3, "Test33", "67777");
			var location = CreateLocation(0);
			var inventoryP = CreateInventory(productId, 10);
			var inventoryP1 = CreateInventory(productId1, 200);
			var pallet = Pallet.CreateForTests("Q1010", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 200, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			DbContext.Categories.Add(category);
			DbContext.Inventories.AddRange(inventoryP, inventoryP1);
			DbContext.Products.AddRange(product, product1, product2, product3);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				UserId = "user",
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)),
				}),(new ProductOnPalletDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)), })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(id, updatedPallet));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.True(resultHandler.IsSuccess);

			Assert.Contains("Paleta Q1010 została zaktualizowana.", resultHandler.Message);
			var result = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == pallet.Id);
			Assert.Equal("Q1010", result.PalletNumber);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			Assert.Equal(updatedPallet.LocationId, result.LocationId);

			Assert.Equal(updatedPallet.ProductsOnPallet.Count, result.ProductsOnPallet.Count);
			foreach (var dto in updatedPallet.ProductsOnPallet)
			{
				Assert.Contains(
					result.ProductsOnPallet,
					p => p.ProductId == dto.ProductId
				);
			}
			foreach (var dto in updatedPallet.ProductsOnPallet)
			{
				var entity = result.ProductsOnPallet
					.Single(p => p.ProductId == dto.ProductId);

				Assert.Equal(dto.Quantity, entity.Quantity);
				Assert.Equal(dto.BestBefore, entity.BestBefore);
			}
			Assert.All(
			result.ProductsOnPallet,
			pop => Assert.Equal(result.Id, pop.PalletId)
			);
			Assert.DoesNotContain(
			result.ProductsOnPallet,
			p => p.ProductId == product2.Id || p.ProductId == product3.Id
			);
			Assert.Equal(PalletStatus.ToPicking, result.Status);

			var inventoryItems = DbContext.Inventories
			.Where(i => i.ProductId == product.Id || i.ProductId == product1.Id)
			.ToList();

			var inventoryProduct = inventoryItems.Single(i => i.ProductId == product.Id);
			var inventoryProduct1 = inventoryItems.Single(i => i.ProductId == product1.Id);

			Assert.Equal(
				updatedPallet.ProductsOnPallet.First(p => p.ProductId == product.Id).Quantity,
				inventoryProduct.Quantity
			);

			Assert.Equal(
				updatedPallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity,
				inventoryProduct1.Quantity
			);

			var history = DbContext.HistoryPallet
			.Where(h => h.PalletId == pallet.Id)
			.ToList();

			Assert.NotEmpty(history);
			Assert.Contains(history, h =>
				h.Reason == ReasonForPallet.Correction &&
				h.PalletStatus == PalletStatus.ToPicking &&
				h.PerformedBy == "user"
			);

			var numberProductDto = updatedPallet.ProductsOnPallet.FirstOrDefault(x => x.ProductId == product.Id).ProductId;
			var numberProductResult = result.ProductsOnPallet.FirstOrDefault(x => x.ProductId == product.Id).ProductId;
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
			Assert.Equal(numberProductDto, numberProductResult);
		}
		[Fact]
		public async Task UpdatePallet_ShouldDecreasingQuantity_WhenProperData()
		{
			//Arange	
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test11", "67777");
			var product2 = CreateProduct(productId2, "Test22", "667777");
			var product3 = CreateProduct(productId3, "Test33", "67777");
			var location = CreateLocation(0);
			var inventoryP = CreateInventory(productId, 1000);
			var inventoryP1 = CreateInventory(productId1, 2000);
			var pallet = Pallet.CreateForTests("Q1010", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 100, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 300, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			DbContext.Categories.Add(category);
			DbContext.Inventories.AddRange(inventoryP, inventoryP1);
			DbContext.Products.AddRange(product, product1, product2, product3);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				UserId = "user",
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					ProductId = product.Id,
					Quantity = 50,
					DateAdded = DateTime.Now,
					BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)),
				}),(new ProductOnPalletDTO
				{
					ProductId = product1.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)), })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(id, updatedPallet));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.True(resultHandler.IsSuccess);

			Assert.Contains("Paleta Q1010 została zaktualizowana.", resultHandler.Message);
			var result = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == pallet.Id);
			Assert.Equal("Q1010", result.PalletNumber);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			Assert.Equal(updatedPallet.LocationId, result.LocationId);

			Assert.Equal(updatedPallet.ProductsOnPallet.Count, result.ProductsOnPallet.Count);
			foreach (var dto in updatedPallet.ProductsOnPallet)
			{
				Assert.Contains(
					result.ProductsOnPallet,
					p => p.ProductId == dto.ProductId
				);
			}
			foreach (var dto in updatedPallet.ProductsOnPallet)
			{
				var entity = result.ProductsOnPallet
					.Single(p => p.ProductId == dto.ProductId);

				Assert.Equal(dto.Quantity, entity.Quantity);
				Assert.Equal(dto.BestBefore, entity.BestBefore);
			}
			Assert.All(
			result.ProductsOnPallet,
			pop => Assert.Equal(result.Id, pop.PalletId)
			);
			Assert.DoesNotContain(
			result.ProductsOnPallet,
			p => p.ProductId == product2.Id || p.ProductId == product3.Id
			);
			Assert.Equal(PalletStatus.ToPicking, result.Status);

			var inventoryItems = DbContext.Inventories
			.Where(i => i.ProductId == product.Id || i.ProductId == product1.Id)
			.ToList();

			var inventoryProduct = inventoryItems.Single(i => i.ProductId == product.Id);
			var inventoryProduct1 = inventoryItems.Single(i => i.ProductId == product1.Id);

			Assert.Equal(
				inventoryP.Quantity - pallet.ProductsOnPallet.First(p => p.ProductId == product.Id).Quantity +
				updatedPallet.ProductsOnPallet.First(p => p.ProductId == product.Id).Quantity,
				inventoryProduct.Quantity
			);

			Assert.Equal(
				inventoryP1.Quantity - pallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity +
				updatedPallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity,
				inventoryProduct1.Quantity
			);

			var history = DbContext.HistoryPallet
			.Where(h => h.PalletId == pallet.Id)
			.ToList();

			Assert.NotEmpty(history);
			Assert.Contains(history, h =>
				h.Reason == ReasonForPallet.Correction &&
				h.PalletStatus == PalletStatus.ToPicking &&
				h.PerformedBy == "user"
			);

			var numberProductDto = updatedPallet.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id).ProductId; 
			var numberProductResult = result.ProductsOnPallet.FirstOrDefault(x => x.ProductId == product.Id).ProductId; 
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
			Assert.Equal(numberProductDto, numberProductResult);
		}
		[Fact]
		public async Task UpdatePallet_ShouldChangeData_WhenProperDataAddTwoNewProducts()
		{
			//Arange	
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test11", "67777");
			var product2 = CreateProduct(productId2, "Test22", "667777");
			var product3 = CreateProduct(productId3, "Test33", "67777");
			var location = CreateLocation(0);
			var pallet = Pallet.CreateForTests("Q1010", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 200, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));

			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1, product2, product3);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				UserId = "user",
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) }),
				(new ProductOnPalletDTO
				{
					ProductId = product2.Id,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) }),
					(new ProductOnPalletDTO
				{
					ProductId = product3.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(id, updatedPallet));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.Contains("Paleta Q1010 została zaktualizowana.", resultHandler.Message);

			var result = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == pallet.Id);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			Assert.Equal(updatedPallet.ProductsOnPallet.Count, result.ProductsOnPallet.Count);
			var numberProductDto = updatedPallet.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id).ProductId; 
			var numberProductResult = result.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id).ProductId; 
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
			Assert.Equal(numberProductDto, numberProductResult);
		}

		[Fact]
		public async Task UpdatePallet_ThrowValidationException_NoNumberProductQuantityZeroWrongBB()
		{
			//Arange	
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test11", "67777");
			var product2 = CreateProduct(productId2, "Test22", "667777");
			var product3 = CreateProduct(productId3, "Test33", "67777");
			var location = CreateLocation(0);
			var pallet = Pallet.CreateForTests("Q1010", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 100, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 300, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));

			DbContext.Categories.Add(category);
			DbContext.Locations.Add(location);
			DbContext.Products.AddRange(product, product1);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act&Assert
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				LocationId = 1,
				Status = PalletStatus.ToPicking,
				UserId = "usert",
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					Quantity = 0,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2024, 5, 4) })
					]
			};
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdatePalletCommand(id, updatedPallet)));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
			Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
			Assert.Contains("Data do spożycia musi być późniejsza niż data dzisiejsza", ex.Message);
		}

		[Fact]
		public async Task UpdatePallet_ThrowValidationException_NoStatusNoLocation()
		{
			//Arange		
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test11", "67777");
			var product2 = CreateProduct(productId2, "Test22", "667777");
			var location = CreateLocation(0);
			var pallet = Pallet.CreateForTests("Q1010", DateTime.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 100, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 300, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)));

			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1, product2);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act&Assert
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				UserId = "user",
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					ProductId = product2.Id,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdatePalletCommand(id, updatedPallet)));
			Assert.Contains("Paleta musi mieć status", ex.Message);
			Assert.Contains("Paleta musi mieć lokalizację", ex.Message);
		}
	}
}
