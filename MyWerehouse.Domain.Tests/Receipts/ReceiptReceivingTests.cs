using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptReceivingTests
	{
		[Fact]
		public void StartReceving_ShouldChangeStatusAndSendDomainEvent_WhenStatusPlanned()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateNewReceipt();
			var date = TestDates.UtcNow;
			var user = "User";
			//Act
			receipt.StartReceiving(date, user);
			//Assert
			Assert.Equal(ReceiptStatus.InProgress, receipt.ReceiptStatus);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.NotNull(historyReceipt);
			Assert.Equal(ReceiptStatus.InProgress, historyReceipt.ReceiptStatus);
			Assert.Equal(user, historyReceipt.UserId);
		}
		[Fact]
		public void StartReceving_ShouldNotChangeData_WhenOnceStarted()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateNewReceipt();
			var date = TestDates.DaysAgo(1);
			var date1 = TestDates.UtcNow;
			var user = "User";
			receipt.StartReceiving(date, user);
			//Act
			receipt.StartReceiving(date1, user);
			//Assert
			Assert.Equal(ReceiptStatus.InProgress, receipt.ReceiptStatus);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.NotNull(historyReceipt);
			Assert.Equal(ReceiptStatus.InProgress, historyReceipt.ReceiptStatus);
			Assert.Equal(date, receipt.ReceiptDateTime);
			Assert.Equal(user, historyReceipt.UserId);
		}

		[Fact]
		public void StartReceving_ShouldChangeStatusAndNoSendDomainEvent_WhenStatusInProgress()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.InProgress);
			var date = TestDates.UtcNow;
			var user = "User";
			//Act
			receipt.StartReceiving(date, user);
			//Assert
			Assert.Equal(ReceiptStatus.InProgress, receipt.ReceiptStatus);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
		}

		[Theory]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		[InlineData(ReceiptStatus.Verified)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Cancelled)]
		[InlineData(ReceiptStatus.Deleted)]
		public void StartReceving_ThrowError_WhenStatusNotAllowed(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var date = TestDates.UtcNow;
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() =>
			receipt.StartReceiving(date, user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(status, receipt.ReceiptStatus);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
		}
		[Fact]
		public void CompletePhysicalReceipt_ShouldChangeStatusAndSendDomainEvent_WhenStatusInProgress()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.InProgress);
			var user = "User";
			//Act
			receipt.CompletePhysicalReceipt(user);
			//Assert
			Assert.Equal(ReceiptStatus.PhysicallyCompleted, receipt.ReceiptStatus);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.NotNull(historyReceipt);
			Assert.Equal(ReceiptStatus.PhysicallyCompleted, historyReceipt.ReceiptStatus);
			Assert.Equal(user, historyReceipt.UserId);
		}
		[Theory]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		[InlineData(ReceiptStatus.Verified)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Cancelled)]
		[InlineData(ReceiptStatus.Deleted)]
		[InlineData(ReceiptStatus.Planned)]
		public void CompletePhysicalReceipt_ThrowError_WhenStatusNotAllowed(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var date = TestDates.UtcNow;
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() =>
			receipt.CompletePhysicalReceipt(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(status, receipt.ReceiptStatus);
			Assert.Empty(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
		}
	}
}
