using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	public class OthersProductOnPalletTests : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public OthersProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
		[Fact]
		public void CheckIsExists_Exists_ReturnTrue()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1010",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.Add(productOnPallet);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1010";
			var productId = 10;
			var result = _productOnPalletRepo.Exists(palletId, productId);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public async Task CheckIsExists_ExistsAsync_ReturnTrue()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1010",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.Add(productOnPallet);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1010";
			var productId = 10;
			var result = await _productOnPalletRepo.ExistsAsync(palletId, productId);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public void CheckIsExistsBadProductId_Exists_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 888;
			//Act
			var result = _productOnPalletRepo.Exists(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task CheckIsExistsBadProductId_ExistsAsync_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1000";
			var productId = 888;
			//Act
			var result = await _productOnPalletRepo.ExistsAsync(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public void CheckIsExistsBadPalletId_Exists_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1999";
			var productId = 10;
			//Act
			var result = _productOnPalletRepo.Exists(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public async Task CheckIsExistsBadPalletId_ExistsAsync_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1999";
			var productId = 10;
			//Act
			var result = await _productOnPalletRepo.ExistsAsync(palletId, productId);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public void RemoveProductsFromPallet_ClearThePallet_ReturnEmpryPallet()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			var productOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1000";
			_productOnPalletRepo.ClearThePallet(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(result);
			Assert.Empty(result.ProductsOnPallet);
		}
		[Fact]
		public async Task RemoveProductsFromPallet_ClearThePalletAsync_ReturnEmpryPallet()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			var productOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1000";
			await _productOnPalletRepo.ClearThePalletAsync(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(result);
			Assert.Empty(result.ProductsOnPallet);
		}
		[Fact]
		public void AddAmount_IncreaseQuantityOnPallet_UpdatedQuantity()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			var productOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			_productOnPalletRepo.IncreaseQuantityOnPallet(palletId, productId, qunatity);
			//Assert
			var palletResult = _context.Pallets
				.Include(p => p.ProductsOnPallet)				
				.FirstOrDefault(p => p.Id == palletId);
			var productOnPalletResult = _context.ProductOnPallet
				.Where(p => p.PalletId == palletId)
				.FirstOrDefault(p => p.ProductId == productId);
			Assert.NotNull(palletResult);
			Assert.NotNull(productOnPalletResult);
			var q1 = productOnPalletResult.Quantity;
			Assert.True(0 < q1);
			Assert.Equal(productOnPallet1.Quantity + qunatity, q1);
		}
		[Fact]
		public async Task AddAmount_IncreaseQuantityOnPalletAsync_UpdatedQuantity()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			var productOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			await _productOnPalletRepo.IncreaseQuantityOnPalletAsync(palletId, productId, qunatity);
			//Assert
			var palletResult = _context.Pallets
				.Include(p => p.ProductsOnPallet)				
				.FirstOrDefault(p => p.Id == palletId);
			var productOnPalletResult = _context.ProductOnPallet
				.Where(p => p.PalletId == palletId)
				.FirstOrDefault(p => p.ProductId == productId);
			Assert.NotNull(palletResult);
			Assert.NotNull(productOnPalletResult);
			var q1 = productOnPalletResult.Quantity;
			Assert.True(0 < q1);
			Assert.Equal(productOnPallet1.Quantity + qunatity, q1);
		}
		[Fact]
		public void AddAmount_DecreaseQuantityOnPallet_UpdatedQuantity()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			var productOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			_productOnPalletRepo.DecreaseQuantityOnPallet(palletId, productId, qunatity);
			//Assert
			var palletResult = _context.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefault(p => p.Id == palletId);
			var productOnPalletResult = _context.ProductOnPallet
				.Where(p => p.PalletId == palletId)
				.FirstOrDefault(p => p.ProductId == productId);
			Assert.NotNull(palletResult);
			Assert.NotNull(productOnPalletResult);
			var q1 = productOnPalletResult.Quantity;
			Assert.True(0 < q1);
			Assert.Equal(productOnPallet1.Quantity - qunatity, q1);			
		}
		[Fact]
		public async Task AddAmount_DecreaseQuantityOnPalletAsync_UpdatedQuantity()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var productOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			var productOnPallet1 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 100,
				DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
			};
			_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			//Act
			var palletId = "Q1000";
			var productId = 10;
			var qunatity = 20;
			await _productOnPalletRepo.DecreaseQuantityOnPalletAsync(palletId, productId, qunatity);
			//Assert
			var palletResult = _context.Pallets
			.Include(p => p.ProductsOnPallet)
			.FirstOrDefault(p => p.Id == palletId);
			var productOnPalletResult = _context.ProductOnPallet
				.Where(p => p.PalletId == palletId)
				.FirstOrDefault(p => p.ProductId == productId);
			Assert.NotNull(palletResult);
			Assert.NotNull(productOnPalletResult);
			var q1 = productOnPalletResult.Quantity;
			Assert.True(0 < q1);
			Assert.Equal(productOnPallet1.Quantity - qunatity, q1);			
		}
	}
}
