using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.PalletTests.Integration
{

	public class UpdatePalletIntegrationTests : TestBase
	{
		private readonly Guid productId = Guid.NewGuid();
		private readonly Guid productId1 = Guid.NewGuid();
		private readonly Guid productId2 = Guid.NewGuid();
		private readonly Guid productId3 = Guid.NewGuid();
		private static Category CreateCategory()
		{
			return new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
		}
		private static Product CreateProduct(Guid id, string name, string sku)
		{
			return Product.CreateForSeed(id, name, sku, TestDates.UtcNow, 1, false, 56);
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
		private static Inventory CreateInventory(Guid id, int quantity)
		{
			return new Inventory
			{
				ProductId = id,
				Quantity = quantity,
				LastUpdated = TestDates.UtcNow.AddDays(-1)
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
			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 200, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
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
				ProductsOnPallet = [ ( new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)),
				}),(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = TestDates.Now,
					BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)), })
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

			var numberProductDto = updatedPallet.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
			var numberProductResult = result.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
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
			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 100, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 300, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
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
				ProductsOnPallet = [ ( new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 50,
					DateAdded = TestDates.Now,
					BestBefore =DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)),
				}),(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)), })
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

			var numberProductDto = updatedPallet.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
			var numberProductResult = result.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
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

			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 200, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));

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
				ProductsOnPallet = [
					(new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),
					(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 4) }),
					(new ProductOnPalletCreateDTO
				{
					ProductId = product2.Id,
					Quantity = 200,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 5, 4) }),
					(new ProductOnPalletCreateDTO
				{
					ProductId = product3.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
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
			var numberProductDto = updatedPallet.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
			var numberProductResult = result.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
			Assert.Equal(numberProductDto, numberProductResult);
		}

		[Fact]
		public async Task UpdatePallet_ShouldKeepDataReceipt_WhenProperData()
		{
			//Arange	
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
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test1", "55555");
			var location = CreateLocation(0);

			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "user", new DateTime(2025, 1, 1), ReceiptStatus.Verified, 1);
			
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, receiptId1, null);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				UserId = "user",
				ProductsOnPallet = [ ( new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
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
			var numberProductDto = updatedPallet.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
			var numberProductResult = result.ProductsOnPallet.Single(x => x.ProductId == product.Id).ProductId;
			var updatedQty = updatedPallet.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			var resultQty = result.ProductsOnPallet.First(x => x.ProductId == product.Id).Quantity;
			Assert.Equal(updatedQty, resultQty);
			Assert.Equal(numberProductDto, numberProductResult);
			Assert.Equal(receiptId1, pallet.ReceiptId);
		}

		[Fact]
		public async Task UpdatePallet_ShouldThrowValidationError_WhenPalletInIssue()
		{
			//Arange	
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
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var category = CreateCategory();
			var product = CreateProduct(productId, "Test", "666666");
			var product1 = CreateProduct(productId1, "Test1", "55555");
			var location = CreateLocation(0);

			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "user", new DateTime(2025, 1, 1), ReceiptStatus.Verified, 1);
			var issueId1 = Guid.Parse("11111111-2111-1111-1111-111111111111");
			var issue = Issue.CreateForSeed(issueId1, 1, 1, new DateTime(2026, 7, 7), DateOnly.MaxValue, "user", IssueStatus.InProgress, null);
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Issues.Add(issue);
			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, receiptId1, issueId1);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			DbContext.Pallets.Add(pallet);
			DbContext.SaveChanges();
			//Act
			var id = pallet.Id;
			var updatedPallet = new EditPalletDTO
			{
				LocationId = location.Id,
				Status = PalletStatus.ToPicking,
				UserId = "user",
				ProductsOnPallet = [ ( new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					]
			};
			var resultHandler = await Mediator.Send(new UpdatePalletCommand(id, updatedPallet));
			//Assert
			Assert.NotNull(resultHandler);
			Assert.False(resultHandler.IsSuccess);
			Assert.Contains("Wskazana paleta jest w wydaniu, nie można jej zmienić bez usunięcia jej z wydania.", resultHandler.Error);
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
			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 100, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 300, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));

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
				ProductsOnPallet = [ ( new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletCreateDTO
				{
					Quantity = 0,
					DateAdded = TestDates.Now,
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
			var pallet = Pallet.CreateForTests("Q1010", TestDates.UtcNow, 1, PalletStatus.Available, null, null);
			pallet.AddProduct(product.Id, 100, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));
			pallet.AddProduct(product1.Id, 300, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(360)));

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
				ProductsOnPallet = [ ( new ProductOnPalletCreateDTO
				{
					ProductId = product.Id,
					Quantity = 100,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletCreateDTO
				{
					ProductId = product1.Id,
					Quantity = 300,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletCreateDTO
				{
					ProductId = product2.Id,
					Quantity = 200,
					DateAdded = TestDates.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdatePalletCommand(id, updatedPallet)));
			Assert.Contains("Paleta musi mieć status", ex.Message);
			Assert.Contains("Paleta musi mieć lokalizację", ex.Message);
		}
	}
}
