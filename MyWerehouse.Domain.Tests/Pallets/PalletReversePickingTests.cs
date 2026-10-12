using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletReversePickingTests
	{
		[Fact]
		public void CreateNewPalletFromReservePicking_ShouldCreateAddDomainEvent()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("potato", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var pallet = PalletTestData.CreatePallet();
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(10), TestDates.DateBestBeforeInMonths(10));
			var user = "User";
			//Act
			pallet.CreateNewPalletFromReservePicking(snapShot, user);
			//Assert
			Assert.Equal(PalletStatus.InStock, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ReversePicking, historyPallet.ReasonMovement);
		}

		[Fact]
		public void AddReversePickedProduct_ShouldAddProduct_WhenBothBestBeforeDatesAreNullEnoughSpace()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId must be
			pallet.AddProduct(product.Id, 5, TestDates.UtcNow, null);
			//Act
			var (RestQuantity, AddedQuantity) = pallet.AddReversePickedProduct(
				product.Id, null, 4, 10, "user", "location");
			//Assert
			Assert.Equal(0, RestQuantity);
			Assert.Equal(4, AddedQuantity);
			Assert.Equal(9, pallet.ProductsOnPallet.Single().Quantity);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ReversePicking, historyPallet.ReasonMovement);

		}
		[Fact]
		public void AddReversePickedProduct_ShouldAddProduct_WhenBothBestBeforeDatesAreTheSameEnoughSpace()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId must be
			pallet.AddProduct(product.Id, 5, TestDates.UtcNow, dateBB);
			//Act
			var (RestQuantity, AddedQuantity) = pallet.AddReversePickedProduct(
				product.Id, dateBB, 4, 10, "user", "location");
			//Assert
			Assert.Equal(0, RestQuantity);
			Assert.Equal(4, AddedQuantity);
			Assert.Equal(9, pallet.ProductsOnPallet.Single().Quantity);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ReversePicking, historyPallet.ReasonMovement);
		}

		[Fact]
		public void AddReversePickedProduct_ShouldAddAndHaveRest_WhenNotEnoughSpace()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId must be
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB);
			//ActAssert
			var (RestQuantity, AddedQuantity) = pallet.AddReversePickedProduct(
				product.Id, dateBB, 60, product.CartonsPerPallet, "user", "location");
			//
			Assert.Equal(10, RestQuantity);
			Assert.Equal(50, AddedQuantity);
			Assert.Equal(100, pallet.ProductsOnPallet.Single().Quantity);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ReversePicking, historyPallet.ReasonMovement);
		}
		[Fact]
		public void AddReversePickedProduct_ShouldFillPallet_WhenQuantityEqualsFreeSpace()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB);
			//Act
			var (RestQuantity, AddedQuantity) = pallet.AddReversePickedProduct(
				product.Id, dateBB, 50, product.CartonsPerPallet, "user", "location");
			//Assert
			Assert.Equal(0, RestQuantity);
			Assert.Equal(50, AddedQuantity);
			Assert.Equal(100, pallet.ProductsOnPallet.Single().Quantity);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ReversePicking, historyPallet.ReasonMovement);
		}
		[Fact]
		public void AddReversePickedProduct_ReturnError_WhenPalletIsEmpty()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			//Act&Assert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB, 10, product.CartonsPerPallet, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.ProductsOnPallet);
			Assert.Empty(pallet.DomainEvents);
		}
		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void AddReversePickedProduct_ReturnError_WhenQuantityNotPositive(int quantity)
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB);
			//Act&Assert
			var ex = Assert.Throws<InvalidQuantityDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB, quantity, product.CartonsPerPallet, "user", "location"));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
			Assert.Equal(50, pallet.ProductsOnPallet.Single().Quantity);
			Assert.Empty(pallet.DomainEvents);
		}
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void AddReversePickedProduct_ReturnError_WhenOnlyOneBestBeforeIsNull(bool productHasDate)
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			DateOnly? productDate = productHasDate ? dateBB : null;
			DateOnly? returnedProductDate = productHasDate ? null : dateBB;
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, productDate);
			//Act&Assert
			var ex = Assert.Throws<NotCorrectDateBestBeforeDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, returnedProductDate, 10, product.CartonsPerPallet, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(50, pallet.ProductsOnPallet.Single().Quantity);
			Assert.Equal(productDate, pallet.ProductsOnPallet.Single().BestBefore);
			Assert.Empty(pallet.DomainEvents);
		}
		[Fact]
		public void AddReversePickedProduct_ReturnError_WhenNoReceiptId()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, null, null);//ReceiptId 
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB);
			//ActAssert
			var ex = Assert.Throws<PalletIsNotForReverseDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB, 10, 10, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}
		[Fact]
		public void AddReversePickedProduct_ReturnError_WhenMoreThanOneProduct()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var dateBB2 = TestDates.DateBestBeforeInMonths(11);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId 
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB1);
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB2);
			//ActAssert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB1, 10, 10, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}

		[Fact]
		public void AddReversePickedProduct_ReturnError_WhenPAlletIsFull()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 50);
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId 
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB1);
			//ActAssert
			var ex = Assert.Throws<PalletCapacityExceededDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB1, 10, 50, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}

		[Fact]
		public void AddReversePickedProduct_ReturnError_WhenNoProductOnPallet()
		{
			//Arrange
			var product1 = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var product2 = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId 
			pallet.AddProduct(product1.Id, 50, TestDates.UtcNow, dateBB1);
			//ActAssert
			var ex = Assert.Throws<ProductNotFoundOnPalletDomainException>(() =>
			pallet.AddReversePickedProduct(product2.Id, dateBB1, 10, 10, "user", "location"));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}

		[Fact]
		public void AddReversePickedProduct_ReturnError_WhenNotTheSameBB()
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB1 = TestDates.DateBestBeforeInMonths(12);
			var dateBB2 = TestDates.DateBestBeforeInMonths(11);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);//ReceiptId 
			pallet.AddProduct(product.Id, 50, TestDates.UtcNow, dateBB1);
			//ActAssert
			var ex = Assert.Throws<NotCorrectDateBestBeforeDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB2, 10, 10, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}


		[Theory]
		[InlineData(PalletStatus.ToIssue)]//1
		[InlineData(PalletStatus.Damaged)]//2
		[InlineData(PalletStatus.OnHold)]//3
		[InlineData(PalletStatus.Loaded)]//4
		[InlineData(PalletStatus.Receiving)]//5
		[InlineData(PalletStatus.LockedForIssue)]//6
		[InlineData(PalletStatus.Picking)]//7
		[InlineData(PalletStatus.ReversePicking)]//8
		[InlineData(PalletStatus.New)]//9
		[InlineData(PalletStatus.Cancelled)]//10
		[InlineData(PalletStatus.Archived)]//11
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.ToPicking)]
		public void AddReversePickedProduct_ThrowException_WhenWrongStatus(PalletStatus status)
		{
			//Arrange
			var product = PalletTestData.CreateProductWithCartonsOnPallet("soup", 2, 100);
			var dateBB = TestDates.DateBestBeforeInMonths(12);
			var pallet = PalletTestData.CreatePalletWithStatus(status);
			//Act
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.AddReversePickedProduct(product.Id, dateBB, 10, 10, "user", "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}
	}
}
