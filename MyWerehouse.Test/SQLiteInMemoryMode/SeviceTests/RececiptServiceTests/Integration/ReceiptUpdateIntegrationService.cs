using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Integration
{
	public class ReceiptUpdateIntegrationService : ReceiptIntegratioCommandService
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
			var initialReceipt = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
				Id = initialReceipt.Id,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
			var result = await _receiptService.UpdateReceiptPalletsAsync(updatingReceipt, userId);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
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
			Assert.DoesNotContain(receiptWithPallets.Pallets, p => p.Id == "Q1000");
			using var arrangeContext = CreateNewContext();
			//Stara paleta(Q1000) powinna być usunięta z bazy
			var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");

			Assert.Null(oldPallet);
			var allPallets = await DbContext.Pallets.ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być

			var allProducts = await DbContext.ProductOnPallet.ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie

			var allMovements = await DbContext.PalletMovements.ToListAsync();
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
			var initialReceipt = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
				Id = initialReceipt.Id,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
								Quantity = 50,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			var result = await _receiptService.UpdateReceiptPalletsAsync(updatingReceipt, userId);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
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

			//Nie powinno tam być palety Q1000
			Assert.DoesNotContain(receiptWithPallets.Pallets, p => p.Id == "Q1000");
			using var arrangeContext = CreateNewContext();
			//Stara paleta(Q1000) powinna być usunięta z bazy
			var oldPallet = await arrangeContext.Pallets.FindAsync("Q1000");

			Assert.Null(oldPallet);
			var allPallets = await DbContext.Pallets.ToListAsync();
			Assert.Single(allPallets); // tylko nowa paleta powinna być

			var allProducts = await DbContext.ProductOnPallet.ToListAsync();
			Assert.Single(allProducts); // jeden produkt na jednej palecie

			var allMovements = await DbContext.PalletMovements.ToListAsync();
			Assert.Single(allMovements); // jeden ruch powinien być utworzony			
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
			var initialReceipt = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
				Id = 999,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
			var result = await _receiptService.UpdateReceiptPalletsAsync(updatingReceipt, userId);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains("Nie znaleziono przyjęcia", result.Message);
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
			var initialReceipt = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
			};
			var initialReceipt1 = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U003",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
				Id = initialReceipt.Id,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
			var result = await _receiptService.UpdateReceiptPalletsAsync(updatingReceipt, userId);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains($"Paleta o numerze {secondPallet.Id} należy do innego przyjęcia o numerze {secondPallet.ReceiptId}", result.Message);
		}
		[Fact]
		public async Task UpdatePalletToReceiptAsync_NonProperDataNoPallet_ReturnInfo()
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
			var initialReceipt = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
			};
			var initialReceipt1 = new Receipt
			{
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U003",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
			DbContext.Receipts.AddRange(initialReceipt, initialReceipt1);
			DbContext.Pallets.AddRange(initialPallet, secondPallet);
			await DbContext.SaveChangesAsync();
			//Act
			var updatingReceipt = new ReceiptDTO
			{
				Id = initialReceipt.Id,
				ClientId = initailCLient.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.Correction,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q2001",
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
			var result = await _receiptService.UpdateReceiptPalletsAsync(updatingReceipt, userId);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Contains($"Nie znaleziono palety o Id: Q2001", result.Message);
		}
	}
}
