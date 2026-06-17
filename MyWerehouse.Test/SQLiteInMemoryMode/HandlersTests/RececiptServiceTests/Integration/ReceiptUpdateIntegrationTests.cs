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

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptUpdateIntegrationTests : TestBase
	{
		//HappyPath może jeszcze dodać z podmianą klienta
		[Fact]
		public async Task UpdatePalletToReceiptAsync_ProperDataOneAddedOneRemoveOnePallets_UpdatedBase()
		{
			//Arrange
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
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false,
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var product = Product.Create("Test", "666666", 1, 10);
			var product1 = Product.Create("Test22", "777777", 1, 10);

			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Receiving, receipt.Id, null);
			initialPallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(initialPallet
				//, secondPallet
				);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{				
				ClientId = client.Id,
				PerformedBy = "U100",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
								//PalletId = secondPallet.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
								//BestBefore = new DateOnly(2027, 3, 3)
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
																	 // Powinna być nowa paleta dodana do bazy (z innym Id niż Q1000)
			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == receipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);//
			Assert.NotEqual(initialPallet.Id, newPallet.Id);

			// Sprawdzenie czy na nowej palecie jest produkt o ProductId = 2 i Quantity = 200
			var newProduct = await DbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == newPallet.Id && p.ProductId == product1.Id);
			Assert.NotNull(newProduct);
			Assert.Equal(200, newProduct.Quantity);

			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.HistoryPallet
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id
				//&& m.Reason == ReasonMovement.Correction //nie bo to nowa paleta dołączana do receipt
				);
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
			//Stara paleta(Q1000) powinna być anulowana
			//var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");
			var oldPallet = await arrangeContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q1000");

			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być

			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie

			var allMovements = await DbContext.HistoryPallet.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allMovements); // jeden ruch powinien być utworzony			
		}
		[Fact]
		public async Task UpdatePalletToReceiptAsync_ProperDataOneAddedOneRemoveOnePalletsChangeQuantity_AddedToBase()
		{
			//Arrange
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
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false,
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			}; 
			var product = Product.Create("Test", "666666", 1, 10);
			var product1 = Product.Create("Test22", "777777", 1, 10);
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
																	 // Powinna być nowa paleta dodana do bazy (z innym Id niż Q1000)
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
			//var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");
			var oldPallet = await arrangeContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q1000");

			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być

			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie

			var allMovements = await DbContext.HistoryPallet.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allMovements);
			// jeden ruch powinien być utworzony	
			////Nie powinno tam być palety Q1000
			//Assert.DoesNotContain(receiptWithPallets.Pallets, p => p.Id == "Q1000");
			//using var arrangeContext = CreateNewContext();
			////Stara paleta(Q1000) powinna być usunięta z bazy
			//var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");

			//Assert.Null(oldPallet);
			//var allPallets = await DbContext.Pallets.ToListAsync();
			//Assert.Single(allPallets); // tylko nowa paleta powinna być

			//var allProducts = await DbContext.ProductOnPallet.ToListAsync();
			//Assert.Single(allProducts); // jeden produkt na jednej palecie

			//var allMovements = await DbContext.PalletMovements.ToListAsync();
			//Assert.Single(allMovements); // jeden ruch powinien być utworzony			
		}

		[Fact]
		public async Task UpdatePalletToReceiptAsync_ProperDataOneAddedOneRemoveOnePalletsChangeQuantityAndAddedNewProduct_AddedToBase()
		{
			//Arrange
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
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false,
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var product = Product.Create("Test", "666666", 1, 10);
			var product1 = Product.Create("Test22", "777777", 1, 10);

			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned, 1);
			
			var initialPallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			initialPallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			
			var secondPallet = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 1, PalletStatus.Available, receipt.Id, null);
			secondPallet.AddProduct(product1.Id, 200, new DateOnly(2027, 3, 3));
			
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = secondPallet.Id,
						PalletNumber = "Q2000",
						LocationId =location.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = product1.Id,
								PalletId = secondPallet.Id,
								Quantity = 50,
								DateAdded = DateTime.Now,
							},
							new()
							{
								ProductId = product.Id,
								PalletId = secondPallet.Id,
								Quantity = 150,
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

			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == secondPallet.Id);

			// powinny być dwa produkty na palecie
			Assert.Equal(2, updatedPallet.ProductsOnPallet.Count);

			// produkt 1: initialProduct1 z ilością 50
			var existingProduct = updatedPallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == product1.Id);
			Assert.NotNull(existingProduct);
			Assert.Equal(50, existingProduct.Quantity);

			// produkt 2: initialProduct (nowy) z ilością 150
			var addedProduct = updatedPallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == product.Id);
			Assert.NotNull(addedProduct);
			Assert.Equal(150, addedProduct.Quantity);

			// --- weryfikacja liczby produktów globalnie w bazie ---
			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Equal(2, allProducts.Count);

			// opcjonalnie: upewnij się, że oba produkty są przypisane do tej samej palety
			Assert.All(allProducts, p => Assert.Equal(secondPallet.Id, p.PalletId));

			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == receipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);//
			Assert.NotEqual("Q1000", newPallet.PalletNumber);

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
			//var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");
			var oldPallet = await arrangeContext.Pallets.FirstOrDefaultAsync(x => x.PalletNumber == "Q1000");

			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być			

			var allMovements = await DbContext.HistoryPallet.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allMovements);

		}
		[Fact]
		public async Task UpdatePalletToReceiptAsync_ChangeProduct_AddToBase()
		{
			//Arrange
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
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false,
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var product = Product.Create("Test", "666666", 1, 10);
			
			var product1 = Product.Create("Test22", "7777", 1, 10);
			
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			DbContext.Receipts.AddRange(receipt);
			DbContext.Pallets.AddRange(pallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = client.Id,
				PerformedBy = "U100",
				ReceiptStatus = ReceiptStatus.Correction,
				RampNumber = 1,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
								PalletId = secondPallet.Id,//tu jest specjalnie bug
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
			var palletChanged = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "Q1000");
			Assert.Equal(product1.Id, palletChanged.ProductsOnPallet.First().ProductId);
			Assert.Equal(200, palletChanged.ProductsOnPallet.First().Quantity);
		}
		//SadPath
		[Fact]
		public async Task UpdatePalletToReceiptAsync_NonProperDataInvalidReceiptId_ReturnInfo()
		{
			//Arrange
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
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false,
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var product = Product.Create("Test", "666666", 1, 10);
			var product1 = Product.Create("Test22", "777777", 1, 10);

			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
				new DateTime(2025, 6, 6), ReceiptStatus.Planned,1);
			
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
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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

		//HappyPath
		[Fact]
		public async Task UpdateReceipt_WhenNewPalletWithoutIdIsProvided_ShouldCreateAndAttachPallet()
		{
			//Arrange
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
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false,
			};
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var product = Product.Create("Test", "666666", 1, 10);
			
			var product1 = Product.Create("Test22", "777777", 1, 10);
			
			var location = new Location
			{
				Id = 9,
				Aisle = 0,
				Bay = 9,
				Height = 0,
				Position = 0
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U001",
				new DateTime(2025, 6, 6), ReceiptStatus.InProgress, 9);
			
			var pallet = Pallet.CreateForTests("Q1000", DateTime.UtcNow, 9, PalletStatus.Available, receipt.Id, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			
			var pallet1 = Pallet.CreateForTests("Q2000", DateTime.UtcNow, 9, PalletStatus.Available, receipt.Id, null);
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
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = pallet.Id,
						PalletNumber = "Q1000",
						LocationId = 9,
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
						LocationId = 9,
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
			//var newPallet = DbContext.Pallets.Find("Q1001");
			//Assert.NotNull(newPallet);
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
			Assert.Equal(9, newPallet.LocationId);
			Assert.Single(newPallet.ProductsOnPallet);

			var productOnPallet = newPallet.ProductsOnPallet.First();
			Assert.Equal(product1.Id, productOnPallet.ProductId);
			Assert.Equal(200, productOnPallet.Quantity);
			var removedPallet = DbContext.Pallets.FirstOrDefault(x=>x.PalletNumber == "Q2000");
			//			Assert.Null(removedPallet);
		}
	}
}
