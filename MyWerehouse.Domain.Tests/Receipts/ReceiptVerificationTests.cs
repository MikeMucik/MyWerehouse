using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptVerificationTests
	{
		[Fact]
		public void VerifiedReceipt_ShoukdChangeStatusPalletAddHistoryAndStock_WhenPhysicallyCompleted()
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.PhysicallyCompleted, 1);
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 2);
			var location1 = ReceiptTestData.CreateLocation(1);
			var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location2.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 20, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var user = "User";
			//Act
			receipt.VerifiedReceipt(user);
			//Assert
			Assert.Equal(ReceiptStatus.Verified, receipt.ReceiptStatus);
			Assert.Equal(PalletStatus.InStock, pallet1.Status);
			var historyPallet1 = Assert.Single(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Received, historyPallet1.ReasonMovement);
			Assert.Equal(PalletStatus.InStock, pallet2.Status);
			var historyPallet2 = Assert.Single(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Received, historyPallet2.ReasonMovement);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(user, historyReceipt.UserId);
			Assert.Equal(2, historyReceipt.DetailDtos.Count);
			var stockChange = Assert.Single(receipt.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Equal(10, stockChange.Changes.Single(x => x.ProductId == product1.Id).Quantity);
			Assert.Equal(20, stockChange.Changes.Single(x => x.ProductId == product2.Id).Quantity);
		}
		[Fact]
		public void VerifiedReceipt_ShoukdChangeStatusPalletAddHistoryAndStock_WhenStockOneProduct()
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.PhysicallyCompleted, 1);
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location2.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product1.Id, 20, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var user = "User";
			//Act
			receipt.VerifiedReceipt(user);
			//Assert
			Assert.Equal(ReceiptStatus.Verified, receipt.ReceiptStatus);
			Assert.Equal(PalletStatus.InStock, pallet1.Status);
			var historyPallet1 = Assert.Single(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Received, historyPallet1.ReasonMovement);
			Assert.Equal(PalletStatus.InStock, pallet2.Status);
			var historyPallet2 = Assert.Single(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Received, historyPallet2.ReasonMovement);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(user, historyReceipt.UserId);
			Assert.Equal(2, historyReceipt.DetailDtos.Count);
			var stockChange = Assert.Single(receipt.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Equal(30, stockChange.Changes.Single(x => x.ProductId == product1.Id).Quantity);
		}
		[Theory]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.New)]
		public void VerifiedReceipt_ThrowError_WhenPhysicallyCompletedButSecondPalletHaveBadStatus(PalletStatus status)
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.PhysicallyCompleted, 1);
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 2);
			var location1 = ReceiptTestData.CreateLocation(1);
			var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(1), location2.Id, status, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 20, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			receipt.VerifiedReceipt(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(receipt.DomainEvents);
			Assert.Equal(ReceiptStatus.PhysicallyCompleted, receipt.ReceiptStatus);
			Assert.Equal(PalletStatus.Receiving, pallet1.Status);
			Assert.Equal(status, pallet2.Status);

			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}
		[Theory]
		[InlineData(ReceiptStatus.Cancelled)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Deleted)]
		[InlineData(ReceiptStatus.Planned)]
		[InlineData(ReceiptStatus.InProgress)]
		public void VerifiedReceipt_ThrowError_WhenNotAllowedStatus(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var pallet = receipt.Pallets.First();
			pallet.ChangeStatus(PalletStatus.Receiving);
			//pallet.Status = PalletStatus.Receiving;
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(()=>
			receipt.VerifiedReceipt(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(receipt.DomainEvents);
		}
		[Fact]
		public void VerifiedReceipt_ThrowError_WhenVerifiedStatus()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.Verified);
			var pallet = receipt.Pallets.First();
			pallet.ChangeStatus(PalletStatus.Receiving);
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<ReceiptAlreadyVerifyDomainException>(() =>
			receipt.VerifiedReceipt(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(receipt.DomainEvents);
		}
	}
}
