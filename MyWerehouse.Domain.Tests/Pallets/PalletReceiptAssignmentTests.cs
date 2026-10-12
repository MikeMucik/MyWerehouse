using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletReceiptAssignmentTests
	{
		[Fact]
		public void AssignToReceipt_AddReceiptIdChangeStatus_WhenStatusNew()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product = PalletTestData.CreateProduct("knives", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(1), null);
			var user = "User";
			var receiptId = Guid.NewGuid();
			//Act
			pallet.AssignToReceipt(receiptId, snapShot, user);
			//Assert
			Assert.Equal(receiptId, pallet.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Received, historyPallet.ReasonMovement);
			Assert.Equal(PalletStatus.Receiving, historyPallet.PalletStatus);
		}
		[Fact]
		public void AssignToReceipt_ThrowError_WhenStatusReceiving()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(PalletStatus.Receiving);
			var product = PalletTestData.CreateProduct("knives", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(1), null);
			var user = "User";
			var receiptId = Guid.NewGuid();
			//Act&Arrange
			var ex = Assert.Throws<AlreadyAssignedDomainException>(() =>
			pallet.AssignToReceipt(receiptId, snapShot, user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents);
			Assert.Equal(PalletStatus.Receiving, pallet.Status);
		}
		[Theory]
		[InlineData(PalletStatus.LockedForIssue)]//1
		[InlineData(PalletStatus.ReversePicking)]//2
		[InlineData(PalletStatus.Available)]//3
		[InlineData(PalletStatus.ToPicking)]//4
		[InlineData(PalletStatus.Archived)]//5
		[InlineData(PalletStatus.Cancelled)]//6
		[InlineData(PalletStatus.Damaged)]//7
		[InlineData(PalletStatus.OnHold)]//8
		[InlineData(PalletStatus.Loaded)]//9
		[InlineData(PalletStatus.InStock)]//10
		[InlineData(PalletStatus.ToIssue)]//11
		[InlineData(PalletStatus.Picking)]//12
		public void AssignToReceipt_ThrowError_WhenStatusWrong(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(status);
			var product = PalletTestData.CreateProduct("knives", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(1), null);
			var user = "User";
			var receiptId = Guid.NewGuid();
			//Act&Arrange
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.AssignToReceipt(receiptId, snapShot, user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents);
			Assert.Equal(status, pallet.Status);
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Receiving)]
		public void DetachFromReceipt_MakeNullReceiptId_WhenStatusReceivingOrNew(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var product = PalletTestData.CreateProduct("knives", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(1), null);
			var user = "User";
			//Act
			pallet.DetachFromReceipt(user, snapShot, ReasonForPallet.CancelReceipt);
			//Assert
			Assert.Null(pallet.ReceiptId);
			Assert.Equal(PalletStatus.Cancelled, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.CancelReceipt, historyPallet.ReasonMovement);
			Assert.Equal(PalletStatus.Cancelled, historyPallet.PalletStatus);
		}
		[Theory]
		[InlineData(PalletStatus.LockedForIssue)]//1
		[InlineData(PalletStatus.ReversePicking)]//2
		[InlineData(PalletStatus.Available)]//3
		[InlineData(PalletStatus.ToPicking)]//4
		[InlineData(PalletStatus.Archived)]//5
		[InlineData(PalletStatus.Cancelled)]//6
		[InlineData(PalletStatus.Damaged)]//7
		[InlineData(PalletStatus.OnHold)]//8
		[InlineData(PalletStatus.Loaded)]//9
		[InlineData(PalletStatus.InStock)]//10
		[InlineData(PalletStatus.ToIssue)]//11
		[InlineData(PalletStatus.Picking)]//12
		public void DetachFromReceipt_ThrowError_WhenStatusWrong(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var product = PalletTestData.CreateProduct("knives", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(1), null);
			var user = "User";
			//Act&Arrange
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.DetachFromReceipt(user, snapShot, ReasonForPallet.CancelReceipt));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents);
			Assert.Equal(status, pallet.Status);
		}
		[Theory]
		[InlineData(PalletStatus.ToIssue)]//1
		[InlineData(PalletStatus.Damaged)]//2
		[InlineData(PalletStatus.OnHold)]//3
		[InlineData(PalletStatus.Loaded)]//4
		[InlineData(PalletStatus.InStock)]//5
		[InlineData(PalletStatus.LockedForIssue)]//6
		[InlineData(PalletStatus.Picking)]//7
		[InlineData(PalletStatus.ReversePicking)]//8
		[InlineData(PalletStatus.Archived)]//9
		[InlineData(PalletStatus.Available)]//10
		[InlineData(PalletStatus.Cancelled)]//11
		[InlineData(PalletStatus.ToPicking)]//12
		public void CanBeCancelled_ReturnFalse_WhenStatusPalletNotAllowed(PalletStatus status)
		{
			//Assert
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			//Act
			var result = pallet.CanBeCancelled();
			//Assert
			Assert.False(result);
		}		
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Receiving)]
		public void CanBeCancelled_ReturnTrue_WhenStatusAllowed(PalletStatus status)
		{
			//Assert
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			//Act
			var result = pallet.CanBeCancelled();
			//Assert
			Assert.True(result);
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Receiving)]
		public void CanBeCancelled_ReturnTrue_WhenStatusAllowedWithTwoHistoryRecords(PalletStatus status)
		{
			//Assert
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var history1 = new HistoryPallet
			{
				Id = 1,
			};
			var history2 = new HistoryPallet
			{
				Id = 2,
			};
			pallet.PalletHistory.Add(history1);
			pallet.PalletHistory.Add(history2);
			//Act
			var result = pallet.CanBeCancelled();
			//Assert
			Assert.True(result);
		}
	}
}
