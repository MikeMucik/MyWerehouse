using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Infrastructure.Repositories;
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test22",
				SKU = "777777",
				Category = category,
				IsDeleted = false,
			};
			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber= 1,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var secondPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(initailCLient);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act

			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber = initialReceipt.ReceiptNumber,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q2000",
						LocationId = initailLocation.Id,
						ReceiptId = initialReceipt.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = initialProduct1.Id,
								PalletId = secondPallet.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == initialReceipt.Id);
			Assert.Equal(initailCLient.Id, updatedReceipt.ClientId); // zmiana klienta
																	 // Powinna być nowa paleta dodana do bazy (z innym Id niż Q1000)
			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == initialReceipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);//
			Assert.NotEqual("Q1000", newPallet.Id);

			// Sprawdzenie czy na nowej palecie jest produkt o ProductId = 2 i Quantity = 200
			var newProduct = await DbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == newPallet.Id && p.ProductId == initialProduct1.Id);
			Assert.NotNull(newProduct);
			Assert.Equal(200, newProduct.Quantity);

			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.PalletMovements
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id && m.Reason == ReasonMovement.Correction);
			Assert.NotNull(movement);
			Assert.Equal("U100", movement.PerformedBy);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == initialReceipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Correction, historyRecipt.StatusAfter);

			var receiptWithPallets = await DbContext.Receipts
				.Include(r => r.Pallets)
				.FirstOrDefaultAsync(r => r.Id == initialReceipt.Id);

			//Nie powinno tam być palety Q1000
			//Assert.DoesNotContain(receiptWithPallets.Pallets, p => p.Id == "Q1000");
			using var arrangeContext = CreateNewContext();
			//Stara paleta(Q1000) powinna być anulowana
			var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");

			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p=>p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być

			var allProducts = await DbContext.ProductOnPallet.Where(x=>x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie

			var allMovements = await DbContext.PalletMovements.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test22",
				SKU = "777777",
				Category = category,
				IsDeleted = false,
			};
			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var secondPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(initailCLient);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber = initialReceipt.ReceiptNumber,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q2000",
						LocationId = initailLocation.Id,
						ReceiptId = initialReceipt.Id,
						ReceiptNumber = initialReceipt.ReceiptNumber,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = initialProduct1.Id,
								PalletId = secondPallet.Id,
								Quantity = 50,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == initialReceipt.Id);
			Assert.Equal(initailCLient.Id, updatedReceipt.ClientId); // zmiana klienta
																	 // Powinna być nowa paleta dodana do bazy (z innym Id niż Q1000)
			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == initialReceipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);//
			Assert.NotEqual("Q1000", newPallet.Id);

			// Sprawdzenie czy na nowej palecie jest produkt o ProductId = 2 i Quantity = 50
			var newProduct = await DbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == newPallet.Id && p.ProductId == initialProduct1.Id);
			Assert.NotNull(newProduct);
			Assert.Equal(50, newProduct.Quantity);

			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.PalletMovements
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id && m.Reason == ReasonMovement.Correction);
			Assert.NotNull(movement);
			Assert.Equal("U100", movement.PerformedBy);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == initialReceipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Correction, historyRecipt.StatusAfter);

			var receiptWithPallets = await DbContext.Receipts
				.Include(r => r.Pallets)
				.FirstOrDefaultAsync(r => r.Id == initialReceipt.Id);

			using var arrangeContext = CreateNewContext();
			//Stara paleta(Q1000) powinna być anulowana
			var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");

			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być

			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie

			var allMovements = await DbContext.PalletMovements.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test22",
				SKU = "777777",
				Category = category,
				IsDeleted = false,
			};
			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var secondPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(initailCLient);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber = initialReceipt.ReceiptNumber,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q2000",
						LocationId = initailLocation.Id,
						ReceiptId = initialReceipt.Id,
						ReceiptNumber = initialReceipt.RampNumber,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = initialProduct1.Id,
								PalletId = secondPallet.Id,
								Quantity = 50,
								DateAdded = DateTime.Now,
							},
							new()
							{
								ProductId = initialProduct.Id,
								PalletId = secondPallet.Id,
								Quantity = 150,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == initialReceipt.Id);
			Assert.Equal(initailCLient.Id, updatedReceipt.ClientId); // zmiana klienta

			var updatedPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == "Q2000");

			// powinny być dwa produkty na palecie
			Assert.Equal(2, updatedPallet.ProductsOnPallet.Count);

			// produkt 1: initialProduct1 z ilością 50
			var existingProduct = updatedPallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == initialProduct1.Id);
			Assert.NotNull(existingProduct);
			Assert.Equal(50, existingProduct.Quantity);

			// produkt 2: initialProduct (nowy) z ilością 150
			var addedProduct = updatedPallet.ProductsOnPallet
				.FirstOrDefault(p => p.ProductId == initialProduct.Id);
			Assert.NotNull(addedProduct);
			Assert.Equal(150, addedProduct.Quantity);

			// --- weryfikacja liczby produktów globalnie w bazie ---
			var allProducts = await DbContext.ProductOnPallet.Where(x => x.Pallet.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Equal(2, allProducts.Count);

			// opcjonalnie: upewnij się, że oba produkty są przypisane do tej samej palety
			Assert.All(allProducts, p => Assert.Equal("Q2000", p.PalletId));

			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == initialReceipt.Id && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);//
			Assert.NotEqual("Q1000", newPallet.Id);

			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.PalletMovements
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id && m.Reason == ReasonMovement.Correction);
			Assert.NotNull(movement);
			Assert.Equal("U100", movement.PerformedBy);

			var historyRecipt = DbContext.HistoryReceipts
				.FirstOrDefault(x => x.ReceiptId == initialReceipt.Id);
			Assert.NotNull(historyRecipt);
			Assert.Equal(ReceiptStatus.Correction, historyRecipt.StatusAfter);

			var receiptWithPallets = await DbContext.Receipts
				.Include(r => r.Pallets)
				.FirstOrDefaultAsync(r => r.Id == initialReceipt.Id);

			using var arrangeContext = CreateNewContext();
			//Stara paleta(Q1000) powinna być anulowana
			var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");

			Assert.Equal(PalletStatus.Cancelled, oldPallet.Status);
			var allPallets = await DbContext.Pallets.Where(p => p.Status != PalletStatus.Cancelled).ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być			

			var allMovements = await DbContext.PalletMovements.Where(x => x.PalletStatus != PalletStatus.Cancelled).ToListAsync();
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
			var cLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
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
				Name = "Test1",
				SKU = "7777",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				Client = cLient,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				Receipt = receipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = product,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var secondPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				Receipt = receipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = product1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(cLient);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.Add(location);
			DbContext.Receipts.AddRange(receipt);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = receipt.Id,
				ReceiptNumber = receipt.ReceiptNumber,
				ClientId = cLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				RampNumber = 1,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q1000",
						LocationId = location.Id,
						ReceiptId = receipt.Id,
						ReceiptNumber = receipt.ReceiptNumber,
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
			var userId = "U100";
			var result = await Mediator.Send(new UpdateReceiptCommand( updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			var palletChanged = DbContext.Pallets.FirstOrDefault(p => p.Id == "Q1000");
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test22",
				SKU = "777777",
				Category = category,
				IsDeleted = false,
			};
			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var secondPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(initailCLient);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var receiptId9 = Guid.Parse("22111111-1111-1111-1111-111111111111");
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = receiptId9,
				ReceiptNumber = 999,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q2000",
						LocationId = initailLocation.Id,
						ReceiptId = initialReceipt.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = initialProduct1.Id,
								PalletId = secondPallet.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains($"Przyjęcie o numerze 999 nie zostało znalezione.", result.Error);
		}
		[Fact]
		public async Task UpdatePalletToReceiptAsync_NonProperDataInvalidPallet_ReturnInfo()
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
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test22",
				SKU = "777777",
				Category = category,
				IsDeleted = false,
			};
			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				Client = initailCLient,				
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
			};
			var receiptId2 = Guid.Parse("21111111-1111-1111-1111-111111111111");
			var initialReceipt1 = new Receipt
			{
				Id = receiptId2,
				ReceiptNumber = 2,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U003",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt1,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var secondPallet = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt =
				initialReceipt1,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = initialProduct1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(initailCLient);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.AddRange(initialReceipt, initialReceipt1);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber = initialReceipt.ReceiptNumber,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q2000",
						LocationId = initailLocation.Id,
						ReceiptId = initialReceipt1.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = initialProduct1.Id,
								PalletId = secondPallet.Id,
								Quantity = 200,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains($"Paleta o numerze {secondPallet.Id} należy do innego przyjęcia.", result.Error);
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
			var product = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = category,
				IsDeleted = false,
			};
			var product1 = new Product
			{
				Name = "Test22",
				SKU = "777777",
				Category = category,
				IsDeleted = false,
			};
			var location = new Location
			{
				Id = 9,
				Aisle = 0,
				Bay = 9,
				Height = 0,
				Position = 0
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber =1,
				Client = client,
				ReceiptStatus = ReceiptStatus.InProgress,
				PerformedBy = "U001",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9
			};			
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				Receipt = receipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = product,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			var pallet1 = new Pallet
			{
				Id = "Q2000",
				DateReceived = DateTime.Now,
				Location = location,
				Status = PalletStatus.Available,
				Receipt = receipt,
				ProductsOnPallet = new List<ProductOnPallet>{ new ProductOnPallet
				{
					Product = product1,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				} }
			};
			DbContext.Clients.AddRange(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location);
			DbContext.Receipts.AddRange(receipt);
			DbContext.Pallets.AddRange(pallet, pallet1);
			await DbContext.SaveChangesAsync();
			//Act
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = receipt.Id,
				ReceiptNumber = receipt.ReceiptNumber,
				ClientId = client.Id,
				PerformedBy = "U100",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 9,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q1000",
						LocationId = 9,
						ReceiptId = receipt.Id,
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
						//Id = "Q1001",
						LocationId = 9,
						ReceiptId = receipt.Id,
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
			var userId = "U100";
			
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			//var newPallet = DbContext.Pallets.Find("Q2001");
			//Assert.NotNull(newPallet);
			var pallets = DbContext.Pallets
				.Where(p => p.ReceiptId == receipt.Id)
				.ToList();

			Assert.Equal(2, pallets.Count);
			var existingPallet = DbContext.Pallets.Find("Q1000");
			Assert.NotNull(existingPallet);
			var newPallet = DbContext.Pallets.Find("Q2001");
			Assert.NotNull(newPallet);

			Assert.Equal(receipt.Id, newPallet.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, newPallet.Status);
			Assert.Equal(9, newPallet.LocationId);
			Assert.Single(newPallet.ProductsOnPallet);

			var productOnPallet = newPallet.ProductsOnPallet.First();
			Assert.Equal(product1.Id, productOnPallet.ProductId);
			Assert.Equal(200, productOnPallet.Quantity);
			var removedPallet = DbContext.Pallets.Find("Q2000");
//			Assert.Null(removedPallet);
		}
	}
}
