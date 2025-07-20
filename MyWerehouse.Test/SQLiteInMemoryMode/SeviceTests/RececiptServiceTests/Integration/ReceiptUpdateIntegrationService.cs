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
	public class ReceiptUpdateIntegrationService:ReceiptIntegratioCommandService
	{
		[Fact]
		public async Task ProperDataOneAddedOneRemoveOnePalletsAndClientFullTest_UpdatePalletToReceiptAsync_AddedToBase()
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
			var address1 = new Address
			{
				City = "1111Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailCLient1 = new Client
			{
				Id = 2,
				Name = "222TestCompany",
				Email = "222123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address1]
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 1,
			};
			var initialProductOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets = [initialPallet]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Id = 1,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.ProductOnPallet.Add(initialProductOnPallet);
			DbContext.Pallets.Add(initialPallet);
			DbContext.Clients.AddRange(initailCLient, initailCLient1);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			
			var updatingReceipt = new ReceiptDTO
			{
				Id = 1,
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						LocationId = 1,
						ReceiptId = 1,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = 1,
								Quantity = 1,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";			
			//Act			
			await _receiptService.UpdateReceiptPalletsAsync(updatingReceipt, userId);

			//Assert
			var updatedReceipt = await DbContext.Receipts.Include(r => r.Pallets).FirstAsync(r => r.Id == 1);
			Assert.Equal(2, updatedReceipt.ClientId); // zmiana klienta

			// Powinna być nowa paleta dodana do bazy (z innym Id niż Q1000)
			var newPallet = await DbContext.Pallets.FirstOrDefaultAsync(p => p.ReceiptId == 1 && p.Status == PalletStatus.Receiving);
			Assert.NotNull(newPallet);
			Assert.NotEqual("Q1000", newPallet.Id);

			// Sprawdzenie czy na nowej palecie jest produkt o ProductId = 1 i Quantity = 1
			var newProduct = await DbContext.ProductOnPallet
				.FirstOrDefaultAsync(p => p.PalletId == newPallet.Id && p.ProductId == 1);
			Assert.NotNull(newProduct);
			Assert.Equal(1, newProduct.Quantity);

			// Sprawdzenie czy utworzono ruch palety
			var movement = await DbContext.PalletMovements
				.FirstOrDefaultAsync(m => m.PalletId == newPallet.Id && m.Reason == ReasonMovement.Received);
			Assert.NotNull(movement);
			Assert.Equal("U100", movement.PerformedBy);

			var receiptWithPallets = await DbContext.Receipts
				.Include(r => r.Pallets)
				.FirstOrDefaultAsync(r => r.Id == 1);

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
	}
}
