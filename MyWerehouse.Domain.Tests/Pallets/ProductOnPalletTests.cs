using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class ProductOnPalletTests
	{
		[Fact]
		public void CreateProductOnPallet_ShouldCreate_WhenValueAboveZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			var quantity = 10;
			//Act
			var productOnPallet = ProductOnPallet.Create(product.Id, pallet.Id, quantity, dateAdded, dateBB);
			//Assert
			Assert.Equal(pallet.Id, productOnPallet.PalletId);
			Assert.Equal(quantity, productOnPallet.Quantity);
			Assert.Equal(dateBB, productOnPallet.BestBefore);
			Assert.Equal(dateAdded, productOnPallet.DateAdded);
		}
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void CreateProductOnPallet_ThrowError_WhenValueNotAboveZero(int quantity)
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			//Act&Assert
			var ex = Assert.Throws<InvalidQuantityDomainException>(() =>
			ProductOnPallet.Create(product.Id, pallet.Id, quantity, dateAdded, dateBB));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
			Assert.Equal(pallet.Id, ex.PalletId);
		}
		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(100)]
		public void SetQuantity_ShouldChange_WhenAmountNotMinus(int quantity)
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act
			productOnPallet.SetQuantity(quantity);
			//Assert
			Assert.Equal(quantity, productOnPallet.Quantity);
		}
		[Fact]
		public void SetQuantity_ThrowError_WhenAmountBelowZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act
			var ex = Assert.Throws<InvalidQuantityDomainException>(() =>
			productOnPallet.SetQuantity(-1));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void IncreaseQuantity_ShouldChange_WhenQuantityAboveZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act
			productOnPallet.IncreaseQuantity(1);
			//Assert
			Assert.Equal(11, productOnPallet.Quantity);
		}
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void IncreaseQuantity_ThrowError_WhenValueIsNotPositive(int quantity)
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act&Assert
			var ex = Assert.Throws<InvalidQuantityDomainException>(()=>
			productOnPallet.IncreaseQuantity(quantity));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void DecreaseQuantity_ShouldChange_WhenQuantityAboveZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act
			productOnPallet.DecreaseQuantity(1);
			//Assert
			Assert.Equal(9, productOnPallet.Quantity);
		}
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void DecreaseQuantity_ThrowError_WhenValueIsNotPositive(int quantity)
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act&Assert
			var ex = Assert.Throws<InvalidQuantityDomainException>(() =>
			productOnPallet.DecreaseQuantity(quantity));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void DecreaseQuantity_ThrowError_WhenResultIsBelowZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			typeof(ProductOnPallet).GetProperty(nameof(ProductOnPallet.Pallet))!
				.SetValue(productOnPallet, pallet);
			//Act&Assert
			var ex = Assert.Throws<InsufficientQuantityDomainException>(() =>
			productOnPallet.DecreaseQuantity(11));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void SetBestBefore_ShouldChange()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateBBnew = TestDates.DateBestBeforeInMonths(12);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act
			productOnPallet.SetBestBefore(dateBBnew);
			//Assert
			Assert.Equal(dateBBnew, productOnPallet.BestBefore);
		}
		[Fact]
		public void SetBestBefore_ShouldChange_WhenNull()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("forks", 3);
			var dateBB = TestDates.DateBestBeforeInMonths(2);
			var dateBBnew = TestDates.DateBestBeforeInMonths(12);
			var dateAdded = TestDates.DaysAgo(2);
			pallet.AddProduct(product.Id, 10, dateAdded, dateBB);
			var productOnPallet = pallet.ProductsOnPallet.Single(x => x.ProductId == product.Id);
			//Act
			productOnPallet.SetBestBefore(null);
			//Assert
			Assert.Null(productOnPallet.BestBefore);
		}
	}
}
