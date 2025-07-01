using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;
using Xunit.Sdk;

namespace MyWerehouse.Test.UnitTestRepo.ProductOnPalletTest
{
	public class UpdateProductOnPalletTests : CommandTestBase
	{
		private readonly ProductOnPalletRepo _productOnPalletRepo;
		public UpdateProductOnPalletTests() : base()
		{
			_productOnPalletRepo = new ProductOnPalletRepo(_context);
		}
	//	[Fact]
	//	public void ChangeQuantity_UpdateProductQuantity_ReturnUpdatedQuantity()
	//	{
	//		var pallet = new Pallet
	//		{
	//			Id = "Q1000",
	//			DateReceived = DateTime.Now,
	//			LocationId = 1,
	//			Status = PalletStatus.Available,
	//			ReceiptId = 10,
	//		};
	//		var productOnPallet = new ProductOnPallet
	//		{
	//			Id = 1,
	//			PalletId = "Q1000",
	//			ProductId = 10,
	//			Quantity = 100,
	//			DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
	//		};
	//		var productOnPallet1 = new ProductOnPallet
	//		{
	//			Id = 2,
	//			PalletId = "Q1000",
	//			ProductId = 20,
	//			Quantity = 100,
	//			DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
	//		};
	//		_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
	//		_context.Pallets.Add(pallet);
	//		_context.SaveChanges();
	//		//Arrange
	//		var palletId = "Q1000";
	//		var productId = 10;
	//		var newQuantity = 25;
	//		//Act
	//		_productOnPalletRepo.UpdateProductQuantity(palletId, productId, newQuantity);	
	//		//Assert
	//		var result = _context.ProductOnPallet
	//			.FirstOrDefault(p=>p.PalletId == palletId && p.ProductId ==  productId);
	//		Assert.NotNull(result);
	//		Assert.Equal(newQuantity, result.Quantity);
	//	}
	//	[Fact]
	//	public void ChangeQuantityNotExistProduct_UpdateProductQuantity_ThrowException()
	//	{
	//		//Arrange&Act&Assert
	//		var palletId = "Q1000";
	//		var productId = 1012;
	//		var newQuantity = 25;			
	//		var ex = Assert.Throws<InvalidDataException>(() => 
	//		_productOnPalletRepo.UpdateProductQuantity(palletId, productId, newQuantity));
	//		Assert.Equal("Produkt nie istnieje na palecie", ex.Message);
	//	}
	//	[Fact]
	//	public async Task ChangeQuantity_UpdateProductQuantityAsync_ReturnUpdatedQuantity()
	//	{
	//		var pallet = new Pallet
	//		{
	//			Id = "Q1000",
	//			DateReceived = DateTime.Now,
	//			LocationId = 1,
	//			Status = PalletStatus.Available,
	//			ReceiptId = 10,
	//		};
	//		var productOnPallet = new ProductOnPallet
	//		{
	//			Id = 1,
	//			PalletId = "Q1000",
	//			ProductId = 10,
	//			Quantity = 100,
	//			DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
	//		};
	//		var productOnPallet1 = new ProductOnPallet
	//		{
	//			Id = 2,
	//			PalletId = "Q1000",
	//			ProductId = 20,
	//			Quantity = 100,
	//			DateAdded = new DateTime(2020, 5, 5, 1, 1, 1, 0),
	//		};
	//		_context.ProductOnPallet.AddRange(productOnPallet, productOnPallet1);
	//		_context.Pallets.Add(pallet);
	//		_context.SaveChanges();
	//		//Arrange
	//		var palletId = "Q1000";
	//		var productId = 10;
	//		var newQuantity = 25;
	//		//Act
	//		await _productOnPalletRepo.UpdateProductQuantityAsync(palletId, productId, newQuantity);
	//		//Assert
	//		var result = _context.ProductOnPallet
	//			.FirstOrDefault(p => p.PalletId == palletId && p.ProductId == productId);
	//		Assert.NotNull(result);
	//		Assert.Equal(newQuantity, result.Quantity);
	//	}
	//	[Fact]
	//	public async Task ChangeQuantityNotExistProduct_UpdateProductQuantityAsync_ThrowException()
	//	{
	//		//Arrange&Act&Assert
	//		var palletId = "Q1000";
	//		var productId = 1012;
	//		var newQuantity = 25;
	//		var ex =await Assert.ThrowsAsync<InvalidDataException>(() =>
	//		_productOnPalletRepo.UpdateProductQuantityAsync(palletId, productId, newQuantity));
	//		Assert.Equal("Produkt nie istnieje na palecie", ex.Message);
	//	}
	}
}
