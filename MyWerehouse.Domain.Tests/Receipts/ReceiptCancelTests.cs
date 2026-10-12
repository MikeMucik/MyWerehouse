using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptCancelTests
	{
		[Theory]
		[InlineData(ReceiptStatus.InProgress)]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		public void CancelReceipt_ShouldCancelAndDomainEvents_WhenAllowedStatusReceiptAndPallet(ReceiptStatus status)
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), status, 1);
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location2.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var user = "User";
			//Act
			receipt.Cancel(user);
			//Assert
			Assert.Equal(ReceiptStatus.Cancelled, receipt.ReceiptStatus);
			Assert.Empty(receipt.Pallets);
			Assert.Null(pallet1.ReceiptId);
			Assert.Null(pallet2.ReceiptId);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(ReceiptStatus.Cancelled, historyReceipt.ReceiptStatus);
			Assert.Equal(user, historyReceipt.UserId);
			var historyReceitDetails1 = historyReceipt.DetailDtos.Single(x => x.PalletId == pallet1.Id);
			Assert.Equal(historyReceitDetails1.LocationId, pallet1.LocationId);
			var historyReceitDetails2 = historyReceipt.DetailDtos.Single(x => x.PalletId == pallet2.Id);
			Assert.Equal(historyReceitDetails2.LocationId, pallet2.LocationId);
			Assert.Equal(PalletStatus.Cancelled, pallet1.Status);
			Assert.Equal(PalletStatus.Cancelled, pallet2.Status);
			Assert.Empty(receipt.DomainEvents.OfType<ChangeStockNotification>());
		}
		[Theory]
		[InlineData(ReceiptStatus.InProgress)]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		public void CancelReceipt_ThrowError_WhenFirstPalletHasNotAllowedStatus(ReceiptStatus status)
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), status, 1);
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location1.Id,PalletStatus.Available,receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location2.Id, PalletStatus.New, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);			
			var user = "User";
			//Act
			var ex = Assert.Throws<CannotCancelReceiptDomainException>(()=>
			receipt.Cancel(user));
			//Assert
			Assert.Equal(status, receipt.ReceiptStatus);
			Assert.NotNull(receipt.Pallets);
			Assert.Equal(2, receipt.Pallets.Count);
			Assert.Equal(PalletStatus.Available, pallet1.Status);
			Assert.NotNull(pallet1.ReceiptId);
			Assert.Equal(PalletStatus.New, pallet2.Status);
			Assert.NotNull(pallet2.ReceiptId);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Empty(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Empty(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
		}
		[Theory]
		[InlineData(ReceiptStatus.InProgress)]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		public void CancelReceipt_ThrowError_WhenSecondPalletHasNotAllowedStatus(ReceiptStatus status)
		{
			//Arrange
			//var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), status, 1);
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location2.Id, PalletStatus.InStock, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var user = "User";
			//Act
			var ex = Assert.Throws<CannotCancelReceiptDomainException>(() =>
			receipt.Cancel(user));
			//Assert
			Assert.Equal(status, receipt.ReceiptStatus);
			Assert.NotNull(receipt.Pallets);
			Assert.Equal(2, receipt.Pallets.Count);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Empty(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Empty(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
		}
		[Theory]
		[InlineData(ReceiptStatus.Planned)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Cancelled)]
		[InlineData(ReceiptStatus.Deleted)]
		public void CancelReceipt_ThrowError_WhenWrongStatus(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var user = "User";
			//Act
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() =>
			receipt.Cancel(user));
			//Assert
			Assert.Equal(status, receipt.ReceiptStatus);
			Assert.NotNull(receipt.Pallets);
			Assert.Equal(2, receipt.Pallets.Count);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
		}
		[Fact]
		public void CancelReceipt_ThrowError_WhenVerifiedStatus()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.Verified);
			var user = "User";
			//Act
			var ex = Assert.Throws<ReceiptAlreadyVerifyDomainException>(() =>
			receipt.Cancel(user));
			//Assert
			Assert.Equal(ReceiptStatus.Verified, receipt.ReceiptStatus);
			Assert.NotNull(receipt.Pallets);
			Assert.Equal(2, receipt.Pallets.Count);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
		}
	}
}
