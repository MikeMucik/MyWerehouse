using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.ReceiptTests.Unit
{
	public class ReceiptServiceTests : TestBase
	{
		
		[Fact]
		public async Task ProperDataOnlyUpdatePallet_UpdatePalletToReceiptAsync_AddedToBase()
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
			var location = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
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
			var client1 = new Client
			{
				Id = 2,
				Name = "222TestCompany",
				Email = "222123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address1]
			};
			var category = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("Test", "666666", 1, 56);

			var product1 = Product.Create("Test", "666666", 1, 56);

			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			var pallet = Pallet.CreateForTests("Q1000", TestDates.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
			pallet.AddProduct(product.Id, 100, new DateOnly(2027, 3, 3));
			
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Pallets.Add(pallet);
			DbContext.Clients.AddRange(client, client1);
			DbContext.Receipts.Add(receipt);
			DbContext.Locations.Add(location);
			await DbContext.SaveChangesAsync();

			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{			
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{
						Id = pallet.Id,
						LocationId = 1,
						Status = PalletStatus.Receiving,
						DateReceived = TestDates.Now,
						ProductsOnPallet = new List<ProductOnPalletCreateDTO>
						{
							new()
							{
								PalletId = pallet.Id,
								ProductId = product1.Id,
								Quantity = 1,
								DateAdded = TestDates.Now,
							}
						}
					}
				}
			};
			//Act						
			await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert
			var result = DbContext.Receipts.SingleOrDefault(x => x.Id == id);
			Assert.NotNull(result);
			Assert.Equal(2, result.ClientId);
			var palletResult = result.Pallets;
			var productResult = palletResult.First();
			Assert.Equal(1, productResult.ProductsOnPallet.First(x => x.ProductId == product1.Id).Quantity);
		}
		
		[Fact]
		public async Task ProperDataOneAddedOneRemoveOnePalletsAndClient_UpdatePalletToReceiptAsync_AddedToBase()
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
			var initialClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialClient1 = new Client
			{
				Name = "222TestCompany",
				Email = "222123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address1]
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var initialProduct = Product.Create("Test", "666666", 1, 56);

			var initialProduct1 = Product.Create("Test", "666666", 1, 56);

			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			DbContext.SaveChanges();
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 1, "U002",
			new DateTime(2025, 6, 6), ReceiptStatus.PhysicallyCompleted, 1);
			
			var initialPallet = Pallet.CreateForTests("Q1000", TestDates.UtcNow, 1, PalletStatus.Receiving, receiptId1, null);
			initialPallet.AddProduct(initialProduct.Id, 100, new DateOnly(2027, 3, 3));
			
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Clients.AddRange(initialClient, initialClient1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.Add(receipt);
			DbContext.Pallets.AddRange(initialPallet);			
			await DbContext.SaveChangesAsync();
			var id = receipt.Id;
			var updatingReceipt = new UpdateReceiptDTO
			{
				ClientId = initialClient1.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<EditPalletInReceiptDTO>
				{
					new()
					{						
						LocationId = initailLocation.Id,
						Status = PalletStatus.Receiving,
						DateReceived = TestDates.Now,
						ProductsOnPallet = new List<ProductOnPalletCreateDTO>
						{
							new()
							{
								ProductId = initialProduct1.Id,
								Quantity = 1,
								DateAdded = TestDates.Now,
							}
						}
					}
				}
			};
			//Act						
			await Mediator.Send(new UpdateReceiptCommand(id, updatingReceipt));
			//Assert

			var result = DbContext.Receipts.Include(p => p.Pallets).ThenInclude(pp => pp.ProductsOnPallet).SingleOrDefault(x => x.Id == id);
			Assert.NotNull(result);
			Assert.Equal(2, result.ClientId);
			Assert.Single(result.Pallets); 
			Assert.Equal("Q1001", result.Pallets.First().PalletNumber);
			await using var freshContext = CreateNewContext();
			var newPallet = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefault(p => p.PalletNumber == "Q1001");
			Assert.NotNull(newPallet);
			Assert.Equal(PalletStatus.Receiving, newPallet.Status);
			Assert.Equal(1, newPallet.LocationId);
			Assert.Equal(receipt.Id, newPallet.ReceiptId);

			var productOnPalletChanged = newPallet.ProductsOnPallet.First();
			var initialPallet1 = DbContext.Pallets.Single(x => x.PalletNumber == "Q1001");
			var product = DbContext.ProductOnPallet.FirstOrDefault(p => p.Id == initialPallet1.ProductsOnPallet.First().Id);
			
			Assert.NotNull(product);
			Assert.Equal(initialPallet1.Id, product.PalletId);
			Assert.Equal(1, product.Quantity);
			Assert.Equal(initialProduct1.Id, product.ProductId);		
		}		
	}
}
