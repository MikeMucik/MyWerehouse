using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletArchiveTests
	{
		[Theory]
		[InlineData(PalletStatus.ToIssue, ReasonForPallet.CancelIssue)]//1
		[InlineData(PalletStatus.Damaged, ReasonForPallet.Correction)]//2
		[InlineData(PalletStatus.OnHold, ReasonForPallet.Correction)]//3
		[InlineData(PalletStatus.Loaded, ReasonForPallet.Loaded)]//4
		[InlineData(PalletStatus.Receiving, ReasonForPallet.CancelReceipt)]//5
		[InlineData(PalletStatus.LockedForIssue, ReasonForPallet.ToLoad)]//6
		[InlineData(PalletStatus.Picking, ReasonForPallet.Picking)]//7
		[InlineData(PalletStatus.ReversePicking, ReasonForPallet.Merge)]//8
		[InlineData(PalletStatus.New, ReasonForPallet.New)]//9
		[InlineData(PalletStatus.Cancelled, ReasonForPallet.Correction)]//10
		[InlineData(PalletStatus.ToPicking, ReasonForPallet.Picking)]//11
		[InlineData(PalletStatus.InStock, ReasonForPallet.Moved)]
		[InlineData(PalletStatus.Available, ReasonForPallet.Moved)]
		public void ToArchive_ShouldChangeStatus(PalletStatus status, ReasonForPallet reason)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(status);
			//Act
			pallet.ToArchive("user", reason, "location");
			//Assert
			Assert.Equal(PalletStatus.Archived, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(reason, historyPallet.ReasonMovement);
		}
		[Fact]
		public void ToArchive_ThrowError_WhenStatusArchived()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(PalletStatus.Archived);
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(()=>
			pallet.ToArchive("user", ReasonForPallet.Correction, "location"));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void CheckIfToArchive_ShouldToArchive_WhenQuantityProductsEqualZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product1 = PalletTestData.CreateProduct("pencils", 3);
			var product2 = PalletTestData.CreateProduct("eggs", 1);
			pallet.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet.AddProduct(product2.Id, 20, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			var productOnPallet1 = pallet.ProductsOnPallet.Single(p=>p.ProductId == product1.Id);
			var productOnPallet2 = pallet.ProductsOnPallet.Single(p=>p.ProductId == product2.Id);
			productOnPallet1.SetQuantity(0);
			productOnPallet2.SetQuantity(0);
			//Act
			pallet.CheckIfToArchive("user", ReasonForPallet.Picking, "location");
			//Assert
			Assert.Equal(PalletStatus.Archived, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Picking, historyPallet.ReasonMovement);
		}
		[Fact]
		public void CheckIfToArchive_ShouldChangeStatusReversePicking_WhenQuantityProductsAboveZero()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product1 = PalletTestData.CreateProduct("pencils", 3);
			var product2 = PalletTestData.CreateProduct("eggs", 1);
			pallet.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet.AddProduct(product2.Id, 20, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			var productOnPallet1 = pallet.ProductsOnPallet.Single(p => p.ProductId == product1.Id);
			var productOnPallet2 = pallet.ProductsOnPallet.Single(p => p.ProductId == product2.Id);
			//productOnPallet1.SetQuantity(0);
			productOnPallet2.SetQuantity(0);
			//Act
			pallet.CheckIfToArchive("user", ReasonForPallet.Picking, "location");
			//Assert
			Assert.Equal(PalletStatus.ReversePicking, pallet.Status);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}
		[Fact]
		public void CheckIfToArchive_ShouldChangeStatus_WhenNoProducts()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();			
			//Act
			pallet.CheckIfToArchive("user", ReasonForPallet.Picking, "location");
			//Assert
			Assert.Equal(PalletStatus.Archived, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Picking, historyPallet.ReasonMovement);
		}
	}
}
