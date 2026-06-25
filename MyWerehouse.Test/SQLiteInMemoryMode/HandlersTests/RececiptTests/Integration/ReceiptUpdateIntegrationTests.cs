using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using FluentValidation;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.RececiptTests.Integration
{

	public class ReceiptUpdateIntegrationTests : TestBase
	{
		private Client CreateClient()
		{
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
			return new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
		}
		private Category CreateCategory(string name)
		{
			return new Category
			{
				Name = name,
				IsDeleted = false
			};
		}
		private Product CreateProduct(string name, string sku)
		{
			return Product.Create(name, sku, 1, 56);
		}
		private Location CreateLocation(int id, int position)
		{
			return new Location
			{
				Id = id,
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}

		[Fact]
		public async Task UpdateReceipt_ShouldCancelOldPalletAndAddNewPallet_WhenNewPalletIsProvided()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Receiving, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(pallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				//ReceiptStatus = ReceiptStatus.Correction,
				//ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						LocationId = location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var result = await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == receipt.Id);
			Assert.Equal(client.Id, updatedReceipt.ClientId);
			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == receipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);
			Assert.NotEqual(pallet.Id, newPallet.Id);
			// Sprawdzenie czy na nowej palecie jest produkt o ProductId = 2 i Quantity = 200
			var newProduct = await DbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == newPallet.Id && p.ProductId == product1.Id);
			Assert.NotNull(newProduct);
			Assert.Equal(200, newProduct.Quantity);
			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.HistoryPallet
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id);
			Assert.NotNull(movement);
			Assert.Equal("U100", movement.PerformedBy);
			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == receipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Correction, historyRecipt.StatusAfter);
			var receiptWithPallets = await DbContext.Receipts
				.Include(r => r.Pallets)
				.FirstOrDefaultAsync(r => r.Id == receipt.Id);
			//Nie powinno tam być palety Q1000
			Assert.DoesNotContain(receiptWithPallets.Pallets, p => p.PalletNumber == "Q1000");
			using var arrangeContext = CreateNewContext();
			var oldPallet = await arrangeContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q1000");
			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko jedna paleta powinna być
			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie
			var allMovements = await DbContext.HistoryPallet.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allMovements); // jeden ruch powinien być utworzony			
		}

		[Fact]
		public async Task UpdateReceipt_ShouldChangeClient_WhenDifferentClientIsProvided()
		{
			//Arrange
			var client = CreateClient();
			var address = new Address
			{
				City = "Cracow",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Dluga",
				Phone = 555555,
				Region = "Małoposlkie",
				StreetNumber = "23/3"
			};
			var clientNew = new Client
			{
				Name = "NewCompany",
				Email = "666@op.pl",
				Description = "Description",
				FullName = "FullNewNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Receiving, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client, clientNew);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(pallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = clientNew.Id,
				PerformedBy = "U100",
				//ReceiptStatus = ReceiptStatus.Correction,
				//ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = pallet.Id,
						LocationId = location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product.Id,
								Quantity = 100,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var result = await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == receipt.Id);
			Assert.Equal(clientNew.Id, updatedReceipt.ClientId);

		}

		[Fact]
		public async Task UpdateReceipt_ShouldCancelRemovedPalletAndUpdateRemainingPalletQuantity()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var secondPallet = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			secondPallet.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(pallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				//ReceiptStatus = ReceiptStatus.Correction,
				//ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = secondPallet.Id,
						PalletNumber = "Q2000",
						LocationId = location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								Quantity = 50,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var result = await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == receipt.Id);
			Assert.Equal(client.Id, updatedReceipt.ClientId); // zmiana klienta
			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == receipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);//
			Assert.NotEqual(pallet.Id, newPallet.Id);
			// Sprawdzenie czy na nowej palecie jest produkt o ProductId = 2 i Quantity = 50
			var newProduct = await DbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == newPallet.Id && p.ProductId == product1.Id);
			Assert.NotNull(newProduct);
			Assert.Equal(50, newProduct.Quantity);
			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.HistoryPallet
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id && m.Reason == ReasonForPallet.Correction);
			Assert.NotNull(movement);
			Assert.Equal("U100", movement.PerformedBy);
			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == receipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Correction, historyRecipt.StatusAfter);
			var receiptWithPallets = await DbContext.Receipts
				.Include(r => r.Pallets)
				.FirstOrDefaultAsync(r => r.Id == receipt.Id);
			using var arrangeContext = CreateNewContext();
			//Stara paleta(Q1000) powinna być anulowana
			var oldPallet = await arrangeContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q1000");
			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być
			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie
			var allMovements = await DbContext.HistoryPallet.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allMovements);
		}

		[Fact]
		public async Task UpdateReceipt_ShouldThrowValidationException_WhenPalletContainsMoreThanOneProduct()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(pallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				//ReceiptStatus = ReceiptStatus.Correction,
				//ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = pallet.Id,
						PalletNumber = "Q1000",
						LocationId =location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								Quantity = 50,
								DateAdded = DateTime.Now,
							},
							new()
							{
								ProductId = product.Id,
								Quantity = 150,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt)));
			Assert.Contains("Paleta w przyjęciu może mieć tylko jeden rodzaj towaru.", ex.Message);
		}
		[Fact]
		public async Task UpdateReceipt_ShouldChangeProductAndQuantity_WhenProductOnPalletIsChanged()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.AddRange(receipt);
			DbContext.Pallets.AddRange(pallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = pallet.Id,
						PalletNumber = "Q1000",
						LocationId = location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var result = await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var palletChanged = await DbContext.Pallets
			.Include(p => p.ProductsOnPallet)
			.FirstOrDefaultAsync(p => p.PalletNumber == "Q1000");
			Assert.NotNull(palletChanged);
			Assert.Single(palletChanged.ProductsOnPallet);
			Assert.Equal(product1.Id, palletChanged.ProductsOnPallet.First().ProductId);
			Assert.Equal(200, palletChanged.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public async Task UpdateReceipt_ReturnErrorInfo_WhenWrongReceiptId()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var secondPallet = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			secondPallet.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(pallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var receiptId9 = Guid.Parse("22111111-1111-1111-1111-111111111111");
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = secondPallet.Id,
						PalletNumber = "Q2000",
						LocationId = location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								PalletId = secondPallet.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var result = await Mediator.Send(new UpdateReceiptCommand(receiptId9, updatingReceipt));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains($" nie zostało znalezione.", result.Error);
		}
		[Fact]
		public async Task UpdateReceipt_ShouldAddAndRemovePallet_WhenNewPalletWithoutIdIsProvided()
		{
			//Arrange
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.InProgress, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var pallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location);
			DbContext.Receipts.AddRange(receipt);
			DbContext.Pallets.AddRange(pallet, pallet1);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = pallet.Id,
						PalletNumber = "Q1000",
						LocationId = 1,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product.Id,
								Quantity = 100,
								DateAdded = DateTime.Now,
							}
						}
					},
					new()
					{
						LocationId = 1,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var result = await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var pallets = DbContext.Pallets
				.Where(p => p.ReceiptId == receipt.Id)
				.ToList();
			Assert.Equal(2, pallets.Count);
			var existingPallet = DbContext.Pallets.FirstOrDefault(x => x.PalletNumber == "Q1000");
			Assert.NotNull(existingPallet);
			var newPallet = DbContext.Pallets.FirstOrDefault(x => x.PalletNumber == "Q2001");
			Assert.NotNull(newPallet);
			Assert.Equal(receipt.Id, newPallet.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, newPallet.Status);
			Assert.Equal(1, newPallet.LocationId);
			Assert.Single(newPallet.ProductsOnPallet);
			var productOnPallet = newPallet.ProductsOnPallet.First();
			Assert.Equal(product1.Id, productOnPallet.ProductId);
			Assert.Equal(200, productOnPallet.Quantity);
			var removedPallet = DbContext.Pallets.FirstOrDefault(x => x.PalletNumber == "Q2000");
			Assert.NotNull(removedPallet);
			Assert.Equal(PalletStatus.Cancelled, removedPallet.Status);
		}
		[Fact]
		public async Task UpdateReceipt_ShouldReturnErrorInfo_WhenPalletListIsEmpty()
		{
			//Arrange	
			var client = CreateClient();
			var category = CreateCategory("Category");
			var product = CreateProduct("Test", "666666");
			var product1 = CreateProduct("Test22", "777777");
			var location = CreateLocation(1, 1);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.InProgress, 1);
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			var pallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location);
			DbContext.Receipts.AddRange(receipt);
			DbContext.Pallets.AddRange(pallet, pallet1);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
				}
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt)));
			Assert.Contains("Przyjęcie musi zawierać przyjęte palety.", ex.Message);
		}
	}
}