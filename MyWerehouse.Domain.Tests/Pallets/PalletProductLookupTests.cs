using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Test;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletProductLookupTests
	{
		[Fact]
		public void ContainsProduct_ReturnTrue()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			pallet.AddProduct(product2.Id, 200, dateCreated1, null);
			//Act
			var result = pallet.ContainsProduct(product1.Id);
			//Assert
			Assert.True(result);
		}
		[Fact]
		public void ContainsProduct_ReturnFalse()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			//Act
			var result = pallet.ContainsProduct(product2.Id);
			//Assert
			Assert.False(result);
		}
		[Fact]
		public void GetProductQuantity_ShouldRetunQuantity_WhenProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			//Act
			var result = pallet.GetProductQuantity(product1.Id);
			//Assert
			Assert.Equal(pallet.ProductsOnPallet
				.Single(x => x.ProductId == product1.Id).Quantity, result);
		}

		[Fact]
		public void GetProductQuantity_ShouldRetunZero_WhenNoProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			//Act
			var result = pallet.GetProductQuantity(product2.Id);
			//Assert
			Assert.Equal(0, result);
		}
		[Fact]
		public void GetProductQuantity_ShouldRetun_WhenProductOnPalletTwoBBAndOther()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateBB1 = TestDates.DateBestBeforeInMonths(1);
			var dateBB2 = TestDates.DateBestBeforeInMonths(2);
			pallet.AddProduct(product1.Id, 100, dateCreated, dateBB1);
			pallet.AddProduct(product1.Id, 100, dateCreated, dateBB2);
			pallet.AddProduct(product2.Id, 10, dateCreated, dateBB1);
			//Act
			var result = pallet.GetProductQuantity(product1.Id);
			//Assert
			Assert.Equal(200, result);
		}
		[Fact]
		public void GetProductOnPallet_ShouldReturnProductOnPallet_WhenNoDate()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			//Act
			var result = pallet.GetProductOnPallet(product1.Id, null);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(100, result.Quantity);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPallet_ShouldReturnProductOnPallet_WhenMethodBBDateHasEarlierDate()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();
				
			var dateBB1 = TestDates.DateBestBeforeInMonths(3);
			var dateBB2 = TestDates.DateBestBeforeInMonths(2);
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), dateBB1);
			//Act
			var result = pallet.GetProductOnPallet(product1.Id, dateBB2);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(100, result.Quantity);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPallet_ShouldReturnProductOnPallet_WhenBestBeforeDatesAreEqual()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateBB = TestDates.DateBestBeforeInMonths(3);
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), dateBB);
			//Act
			var result = pallet.GetProductOnPallet(product1.Id, dateBB);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(100, result.Quantity);
			Assert.Equal(dateBB, result.BestBefore);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPallet_ThrowError_WhenBestBeforeRequiredAndProductHasNoDate()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateBB = TestDates.DateBestBeforeInMonths(3);
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			//Act&Assert
			var ex = Assert.Throws<ProductNotFoundOnPalletDomainException>(() =>
			pallet.GetProductOnPallet(product1.Id, dateBB));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void GetProductOnPallet_ThrowError_WhenProductBBDateHasEarlierDate()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();

			var dateBB1 = TestDates.DateBestBeforeInMonths(2);
			var dateBB2 = TestDates.DateBestBeforeInMonths(3);
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), dateBB1);
			//Act&Assert
			var ex = Assert.Throws<ProductNotFoundOnPalletDomainException>(()=>
			pallet.GetProductOnPallet(product1.Id, dateBB2));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);			
		}
		[Fact]
		public void GetProductOnPallet_ReturnException_WhenNoProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			//Act&Assert
			var ex = Assert.Throws<ProductNotFoundOnPalletDomainException>(()
				=> pallet.GetProductOnPallet(product2.Id, null));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void GetProductOnPallet_ReturnException_WhenTwoProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			pallet.AddProduct(product2.Id, 100, TestDates.DaysAgo(1), null);
			//Act&Assert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(()
				=> pallet.GetProductOnPallet(product2.Id, null));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void GetProductOnPallet_ReturnException_WhenProductOnPalletWithTwoDates()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			//var product2 = PalletTestData.CreateProduct("spoons", 1);
			var date1 = TestDates.DateBestBeforeInMonths(2);
			var date2 = TestDates.DateBestBeforeInMonths(4);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), date1);
			pallet.AddProduct(product1.Id, 50, TestDates.DaysAgo(1), date2);
			//Act&Assert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(()
				=> pallet.GetProductOnPallet(product1.Id, date2));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}

		[Fact]
		public void GetProductOnPalletForReverse_ShouldReturnProductOnPallet_WhenOk()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			//Act
			var result = pallet.GetProductOnPalletForReverse(product1.Id, null);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(100, result.Quantity);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPalletForReverse_ReturnException_WhenNoProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			//Act&Assert
			var ex = Assert.Throws<ProductNotFoundOnPalletDomainException>(()
				=> pallet.GetProductOnPalletForReverse(product2.Id, null));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void GetProductOnPalletForReverse_ShouldReturnProductOnPallet_WhenTwoProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), null);
			pallet.AddProduct(product2.Id, 100, TestDates.DaysAgo(1), null);
			//Act
			var result = pallet.GetProductOnPalletForReverse(product1.Id, null);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(100, result.Quantity);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPalletForReverse_ShouldReturnProductOnPallet_WhenProductOnPalletWithTwoDates()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("milk", 1);
			var date1 = TestDates.DateBestBeforeInMonths(2);
			var date2 = TestDates.DateBestBeforeInMonths(4);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), date1);
			pallet.AddProduct(product1.Id, 50, TestDates.DaysAgo(1), date2);			
			//Act
			var result = pallet.GetProductOnPalletForReverse(product1.Id, date2);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(50, result.Quantity);
			Assert.Equal(date2, result.BestBefore);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPalletForReverse_ShouldReturnProductOnPalletWithNoDate_WhenTwoProductOnPalletOnlyOneHasDate()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("milk", 1);
			var date1 = TestDates.DateBestBeforeInMonths(2);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), date1);
			pallet.AddProduct(product1.Id, 50, TestDates.DaysAgo(1), null);
			//Act
			var result = pallet.GetProductOnPalletForReverse(product1.Id, null);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(product1.Id, result.ProductId);
			Assert.Equal(50, result.Quantity);
			Assert.Null(result.BestBefore);
			Assert.Equal(TestDates.DaysAgo(1), result.DateAdded);
		}
		[Fact]
		public void GetProductOnPalletForReverse_ThrowError_WhenTwoProductOnPalletTheSameBB()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("milk", 1);
			var date1 = TestDates.DateBestBeforeInMonths(2);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 100, TestDates.DaysAgo(1), date1);
			pallet.AddProduct(product1.Id, 50, TestDates.DaysAgo(1), date1);
			//Act&Assert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(()=>
			pallet.GetProductOnPalletForReverse(product1.Id, date1));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);			
		}
	}
}
