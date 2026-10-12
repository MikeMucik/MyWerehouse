using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletProductModificationTests
	{
		[Fact]
		public void AddProduct_ShouldAdd_WhenOk()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.Now;
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			//Act
			pallet.AddProduct(product.Id, 100, dateCreated, dateBB);
			//Assert
			Assert.NotEmpty(pallet.ProductsOnPallet);
			Assert.Equal(product.Id, pallet.ProductsOnPallet.Single().ProductId);
			Assert.Equal(100, pallet.ProductsOnPallet.Single().Quantity);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.Single().DateAdded);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.Single().BestBefore);
		}
		[Theory]
		//[InlineData(0)]
		[InlineData(-1)]
		public void AddProduct_ReturnThrow_WhenQuantityNotAllowed(int quantity)
		{
			//Arrange
			var product = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.Now;
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			//Act&Arrange
			var ex = Assert.Throws<InvalidQuantityDomainException>(() => pallet.AddProduct(product.Id, quantity, dateCreated, dateBB));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void AddProduct_ReturnThrow_WhenDatesAreWrong()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.Now;
			var dateBB = TestDates.DateBestBeforeInMonths(-1);
			//Act&Arrange
			var ex = Assert.Throws<WrongDateDomainException>(() => pallet.AddProduct(product.Id, 100, dateCreated, dateBB));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldAdd_WhenTheSameBBAndProduct()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.Now;
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			pallet.AddProduct(product.Id, 100, dateCreated, dateBB);
			//Act
			pallet.AddOrIncreaseProductQuantity(product.Id, 20, TestDates.Now, dateBB);
			//Assert
			Assert.NotEmpty(pallet.ProductsOnPallet);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.Single().DateAdded);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.Single().BestBefore);
			Assert.Equal(120, pallet.ProductsOnPallet.Single().Quantity);
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldAdd_WhenTheSameProductAndOtherBB()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("milk", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			var dateBB1 = TestDates.DateBestBeforeInMonths(2);
			pallet.AddProduct(product.Id, 100, dateCreated, dateBB);
			//Act
			pallet.AddOrIncreaseProductQuantity(product.Id, 20, dateCreated1, dateBB1);
			//Assert
			Assert.NotEmpty(pallet.ProductsOnPallet);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.First(x => x.Quantity == 100).DateAdded);
			Assert.Equal(dateCreated1, pallet.ProductsOnPallet.First(x => x.Quantity == 20).DateAdded);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.First(x => x.Quantity == 100).BestBefore);
			Assert.Equal(dateBB1, pallet.ProductsOnPallet.First(x => x.Quantity == 20).BestBefore);
			Assert.Equal(120, pallet.ProductsOnPallet.Where(x => x.ProductId == product.Id).Sum(x => x.Quantity));
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldAdd_WhenOtherProduct()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("milk", 1);
			var product1 = PalletTestData.CreateProduct("eggs", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			var dateBB1 = TestDates.DateBestBeforeInMonths(2);
			pallet.AddProduct(product.Id, 100, dateCreated, dateBB);
			//Act
			pallet.AddOrIncreaseProductQuantity(product1.Id, 20, dateCreated1, dateBB1);
			//Assert
			Assert.NotEmpty(pallet.ProductsOnPallet);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.First(x => x.Quantity == 100).DateAdded);
			Assert.Equal(dateCreated1, pallet.ProductsOnPallet.First(x => x.Quantity == 20).DateAdded);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.First(x => x.Quantity == 100).BestBefore);
			Assert.Equal(dateBB1, pallet.ProductsOnPallet.First(x => x.Quantity == 20).BestBefore);
			Assert.Equal(2, pallet.ProductsOnPallet.Count);
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldAdd_WhenAddedTwoProduct()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("milk", 1);
			var product2 = PalletTestData.CreateProduct("eggs", 1);
			var product3 = PalletTestData.CreateProduct("butter", 3);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			var dateBB1 = TestDates.DateBestBeforeInMonths(2);
			pallet.AddProduct(product1.Id, 100, dateCreated, dateBB);
			//Act
			pallet.AddOrIncreaseProductQuantity(product2.Id, 20, dateCreated1, dateBB1);
			pallet.AddOrIncreaseProductQuantity(product3.Id, 20, dateCreated1, dateBB);
			//Assert
			Assert.NotEmpty(pallet.ProductsOnPallet);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.First(x => x.Quantity == 100).DateAdded);
			Assert.Equal(dateCreated1, pallet.ProductsOnPallet.First(x => x.Quantity == 20).DateAdded);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.First(x => x.ProductId == product1.Id).BestBefore);
			Assert.Equal(dateBB1, pallet.ProductsOnPallet.First(x => x.ProductId== product2.Id).BestBefore);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.First(x => x.ProductId== product3.Id).BestBefore);
			Assert.Equal(3, pallet.ProductsOnPallet.Count);
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldAdd_WhenAddedWithNullBB()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			//Act
			pallet.AddOrIncreaseProductQuantity(product1.Id, 20, dateCreated1, null);
			pallet.AddOrIncreaseProductQuantity(product2.Id, 20, dateCreated1, null);
			//Assert
			Assert.NotEmpty(pallet.ProductsOnPallet);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.First(x => x.Quantity == 120).DateAdded);
			Assert.Equal(dateCreated1, pallet.ProductsOnPallet.First(x => x.Quantity == 20).DateAdded);
			Assert.Equal(2, pallet.ProductsOnPallet.Count);
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldThrow_WhenAddedZero()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			//Act
			var ex = Assert.Throws<InvalidQuantityDomainException>(()=>
			pallet.AddOrIncreaseProductQuantity(product1.Id, 0, dateCreated1, null));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);			
		}
		[Fact]
		public void AddOrIncreaseProductQuantity_ShouldThrow_WhenAddedMinusQuantity()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePallet();
			var dateCreated = TestDates.DaysAgo(1);
			var dateCreated1 = TestDates.DaysAgo(0);
			pallet.AddProduct(product1.Id, 100, dateCreated, null);
			//Act
			var ex = Assert.Throws<InvalidQuantityDomainException>(() =>
			pallet.AddOrIncreaseProductQuantity(product1.Id, -10, dateCreated1, null));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);			
		}
		[Fact]
		public void ReplaceProducts_ShouldReplace_WhenTwodiffrentProducts()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var dateAdded = TestDates.DaysAgo(1);
			var dateAdding = TestDates.Now;
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var dateBB2 = TestDates.DateBestBeforeInMonths(13);
			var pallet = PalletTestData.CreatePallet();
			var productOld = ProductOnPallet.Create(product1.Id, pallet.Id, 15, dateAdded, dateBB1);
			pallet.AddProduct(product1.Id, 15, dateAdded, dateBB1);		
			var productToReplace = ProductOnPallet.Create(product2.Id, pallet.Id, 10, dateAdding, dateBB2);
			var updatingList = new List<ProductOnPallet>();
			updatingList.Add(productToReplace);
			//Act
			pallet.ReplaceProducts(updatingList);
			//Assert
			Assert.DoesNotContain(pallet.ProductsOnPallet,	x => x.ProductId == product1.Id);
			Assert.Same(productToReplace, Assert.Single(pallet.ProductsOnPallet));
			Assert.Contains(productToReplace, pallet.ProductsOnPallet);
		}
		[Fact]
		public void ReplaceProducts_ShouldAddNotChange_WhenTwodiffrentBB()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var dateAdded = TestDates.DaysAgo(1);
			var dateAdding = TestDates.Now;
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var dateBB2 = TestDates.DateBestBeforeInMonths(13);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 15, dateAdded, dateBB1);

			var productUnchanged = ProductOnPallet.Create(product1.Id, pallet.Id, 15, dateAdding, dateBB1);
			var productToReplace = ProductOnPallet.Create(product1.Id, pallet.Id, 10, dateAdding, dateBB2);
			var updatingList = new List<ProductOnPallet> {productUnchanged, productToReplace};
			//Act
			pallet.ReplaceProducts(updatingList);
			//Assert
			Assert.Contains(pallet.ProductsOnPallet, x => x.ProductId == product1.Id);
			Assert.Same(productToReplace, pallet.ProductsOnPallet.First(x=>x.Quantity == productToReplace.Quantity));
			Assert.Equal(2, pallet.ProductsOnPallet.Count);
			Assert.Equal(15, pallet.ProductsOnPallet.Single(x => x.ProductId == product1.Id && x.BestBefore == dateBB1).Quantity);
		}
		[Fact]
		public void ReplaceProducts_ShouldClearPallet_WhenListEmpty()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var dateAdded = TestDates.DaysAgo(1);
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 15, dateAdded, dateBB1);

			var updatingList = new List<ProductOnPallet>();
			//Act
			pallet.ReplaceProducts(updatingList);
			//Assert
			Assert.Empty(pallet.ProductsOnPallet);
		}
		[Fact]
		public void ReplaceProducts_ShouldChangeQuantity_WhenTheSameProduct()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var dateAdded = TestDates.DaysAgo(1);
			var dateAdding = TestDates.Now;
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var dateBB2 = TestDates.DateBestBeforeInMonths(12);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 15, dateAdded, dateBB1);
			var productToReplace = ProductOnPallet.Create(product1.Id, pallet.Id, 10, dateAdding, dateBB2);
			var updatingList = new List<ProductOnPallet> { productToReplace};
			//Act
			pallet.ReplaceProducts(updatingList);
			//Assert
			Assert.Equal(productToReplace.Quantity, pallet.ProductsOnPallet.Single().Quantity);
		}
		[Fact]
		public void ReplaceProducts_ShouldUpdateOnlyOneQuantity_WhenTwoDifferentProducts()
		{
			//Arrange
			var product1 = PalletTestData.CreateProduct("knives", 1);
			var product2 = PalletTestData.CreateProduct("spoons", 1);
			var dateAdded = TestDates.DaysAgo(1);
			var dateAdding = TestDates.Now;
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var dateBB2 = TestDates.DateBestBeforeInMonths(13);
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product1.Id, 15, dateAdded, dateBB1);
			pallet.AddProduct(product2.Id, 20, dateAdded, dateBB2);
			var productOld1 = pallet.ProductsOnPallet.Single(x => x.ProductId == product1.Id);
			var productOld2 = pallet.ProductsOnPallet.Single(x => x.ProductId == product2.Id);
			var productUnchanged = ProductOnPallet.Create(product1.Id, pallet.Id, 15, dateAdding, dateBB1);
			var productToReplace = ProductOnPallet.Create(product2.Id, pallet.Id, 10, dateAdding, dateBB2);
			var updatingList = new List<ProductOnPallet> { productUnchanged, productToReplace };
			//Act
			pallet.ReplaceProducts(updatingList);
			//Assert
			Assert.Equal(2, pallet.ProductsOnPallet.Count);
			var firstProduct = Assert.Single(pallet.ProductsOnPallet, x => x.ProductId == product1.Id);
			var secondProduct = Assert.Single(pallet.ProductsOnPallet, x => x.ProductId == product2.Id);
			Assert.Same(productOld1, firstProduct);
			Assert.Same(productOld2, secondProduct);
			Assert.Equal(15, firstProduct.Quantity);
			Assert.Equal(10, secondProduct.Quantity);
			Assert.Equal(dateBB1, firstProduct.BestBefore);
			Assert.Equal(dateBB2, secondProduct.BestBefore);
		}

	}
}
