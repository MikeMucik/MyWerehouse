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

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PalletServiceTests.Integration
{
	public class UpdatePalletIntegrationServiceTests : TestBase
	{
		[Fact]
		public async Task ProperDataAddChangeItems_UpdatePalletAsync_ChangeDataIncreasingQuantity()
		{
			//Arange	
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
			var product1 = new Product
			{
				Name = "Test11",
				SKU = "67777",
				Category = category,
				IsDeleted = false,
			};
			var product2 = new Product
			{
				Name = "Test22",
				SKU = "67777",
				Category = category,
				IsDeleted = false,
			};
			var product3 = new Product
			{
				Name = "Test33",
				SKU = "67777",
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
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = [
					new ProductOnPallet
					{
						Product =product,
						Quantity = 10,
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)),
						DateAdded = DateTime.UtcNow,
					},
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 200,
						DateAdded = DateTime.Now,
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360))
					}               ]
			};
			var inventoryP = new Inventory
			{
				Product = product,
				Quantity = 10
			};
			var inventoryP1 = new Inventory
			{
				Product = product1,
				Quantity = 200
			};

			DbContext.Inventories.AddRange(inventoryP, inventoryP1);
			DbContext.Products.AddRange(product, product1, product2, product3);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1010",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product).Id,
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)),
				}),(new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product1).Id,
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)), })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(updatedPallet, "user"));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.True(resultHandler.IsSuccess);
			Assert.Contains("Q1010", resultHandler.Message);

			Assert.Contains("Paleta Q1010 została zaktualizowana.", resultHandler.Message);
			var result = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == pallet.Id);
			Assert.Equal("Q1010", result.Id);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			//Assert.Equal(updatedPallet.DateReceived, result.DateReceived);
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

			var history = DbContext.PalletMovements
			.Where(h => h.PalletId == pallet.Id)
			.ToList();

			Assert.NotEmpty(history);
			Assert.Contains(history, h =>
				h.Reason == ReasonMovement.Correction &&
				h.PalletStatus == PalletStatus.ToPicking &&
				h.PerformedBy == "user"
			);

			//var numberProductDto = updatedPallet.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id); 
			//var numberProductResult = result.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id); 
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
		}
		[Fact]
		public async Task ProperDataAddChangeItems_UpdatePalletAsync_ChangeDataDecreasingQuantity()
		{
			//Arange	
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
			var product1 = new Product
			{
				Name = "Test11",
				SKU = "67777",
				Category = category,
				IsDeleted = false,
			};
			var product2 = new Product
			{
				Name = "Test22",
				SKU = "67777",
				Category = category,
				IsDeleted = false,
			};
			var product3 = new Product
			{
				Name = "Test33",
				SKU = "67777",
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
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = [
					new ProductOnPallet
					{
						Product =product,
						Quantity = 100,
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360)),
						DateAdded = DateTime.UtcNow,
					},
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 300,
						DateAdded = DateTime.Now,
						BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(360))
					}               ]
			};
			var inventoryP = new Inventory
			{
				Product = product,
				Quantity = 1000
			};
			var inventoryP1 = new Inventory
			{
				Product = product1,
				Quantity = 2000
			};

			DbContext.Inventories.AddRange(inventoryP, inventoryP1);
			DbContext.Products.AddRange(product, product1, product2, product3);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1010",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product).Id,
					ProductId = product.Id,
					Quantity = 50,
					DateAdded = DateTime.Now,
					BestBefore =DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)),
				}),(new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product1).Id,
					ProductId = product1.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366)), })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(updatedPallet, "user"));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.True(resultHandler.IsSuccess);
			Assert.Contains("Q1010", resultHandler.Message);

			Assert.Contains("Paleta Q1010 została zaktualizowana.", resultHandler.Message);
			var result = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == pallet.Id);
			Assert.Equal("Q1010", result.Id);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			//Assert.Equal(updatedPallet.DateReceived, result.DateReceived);
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
				inventoryP.Quantity -pallet.ProductsOnPallet.First(p=>p.ProductId == product.Id).Quantity+
				updatedPallet.ProductsOnPallet.First(p=>p.ProductId == product.Id).Quantity,
				inventoryProduct.Quantity
			);

			Assert.Equal(
				inventoryP1.Quantity - pallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity +
				updatedPallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity,
				inventoryProduct1.Quantity
			);

			var history = DbContext.PalletMovements
			.Where(h => h.PalletId == pallet.Id)
			.ToList();

			Assert.NotEmpty(history);
			Assert.Contains(history, h =>
				h.Reason == ReasonMovement.Correction &&
				h.PalletStatus == PalletStatus.ToPicking &&
				h.PerformedBy == "user"
			);

			//var numberProductDto = updatedPallet.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id); 
			//var numberProductResult = result.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id); 
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
		}
		[Fact]
		public async Task ProperDataAddTwoNewItems_UpdatePalletAsync_ChangeData()
		{
			//Arange	
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
			var product1 = new Product
			{
				Name = "Test11",
				SKU = "67777",
				Category = category,
				IsDeleted = false,
			};
			var product2 = new Product
			{
				Name = "Test22",
				SKU = "67777",
				Category = category,
				IsDeleted = false,
			};
			var product3 = new Product
			{
				Name = "Test33",
				SKU = "67777",
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
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet { Product =product, Quantity =10,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
				,DateAdded = DateTime.UtcNow,
				}, new ProductOnPallet  {Product = product1,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
			}]
			};
			DbContext.Products.AddRange(product, product1, product2, product3);
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1010",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product).Id,
					PalletId = "Q1010",
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product1).Id,
					PalletId = "Q1010",
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) }),
				(new ProductOnPalletDTO
				{
					PalletId = "Q1010",
					ProductId = product2.Id,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) }),
					(new ProductOnPalletDTO
				{
					PalletId = "Q1010",
					ProductId = product3.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(updatedPallet, "user"));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.Contains("Paleta Q1010 została zaktualizowana.", resultHandler.Message);

			var result = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Single(x => x.Id == pallet.Id);
			Assert.NotNull(result);
			Assert.Equal(updatedPallet.Status, result.Status);
			Assert.Equal(updatedPallet.ProductsOnPallet.Count, result.ProductsOnPallet.Count);
			//var numberProductDto = updatedPallet.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id); 
			//var numberProductResult = result.ProductsOnPallet.FirstOrDefault(x=>x.ProductId == product.Id); 
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
		}

		[Fact]
		public async Task NonProperDataProduct_UpdatePalletAsync_ThrowException()
		{
			//Arange	
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
			var product1 = new Product
			{
				Name = "Test11",
				SKU = "67777",
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
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet { Product =product, Quantity =10,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
				,DateAdded = DateTime.UtcNow,
				}, new ProductOnPallet  {Product = product1,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
			}]
			};
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act&Assert
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1010",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product).Id,
					PalletId = "Q1010",
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					Id = pallet.ProductsOnPallet.FirstOrDefault(p=>p.Product == product1).Id,
					PalletId = "Q1010",
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					//PalletId = "Q",
					//ProductId = 30,
					Quantity = 0,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2024, 5, 4) })
					]
			};
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdatePalletCommand(updatedPallet, "user")));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
			Assert.Contains("Ilość produktu musi być większa od zera", ex.Message);
			Assert.Contains("Data do spożycia musi być późniejsza niż data dzisiejsza", ex.Message);
		}

		[Fact]
		public async Task NonProperDataPallet_UpdatePalletAsync_ThrowException()
		{
			//Arange			
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
			var product1 = new Product
			{
				Name = "Test11",
				SKU = "67777",
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
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new ProductOnPallet { Product =product, Quantity =10,
					BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
				,DateAdded = DateTime.UtcNow,
				}, new ProductOnPallet  {Product = product1,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(366))
			}]
			};
			DbContext.Locations.Add(location);
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act&Assert
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1010",
				//DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				//LocationId = 1,
				//Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					//Id = product1.Id,
					//PalletId = "Q2000",
					ProductId = 10,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					//Id = product2.Id,
					//PalletId = "Q2000",
					ProductId = 20,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					//Id = 300,
					PalletId = "Q2000",
					ProductId = 30,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdatePalletCommand(updatedPallet, "user")));
			Assert.Contains("Paleta musi mieć status", ex.Message);
			Assert.Contains("Paleta musi mieć datę utworzenia", ex.Message);
			Assert.Contains("Paleta musi mieć lokalizację", ex.Message);
		}
	}
}
