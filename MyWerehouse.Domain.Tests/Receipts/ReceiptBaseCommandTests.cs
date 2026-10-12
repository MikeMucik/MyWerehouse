using MyWerehouse.Domain.Clients.ClientsExceptions;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptBaseCommandTests
	{
		[Fact]
		public void CreateReceipt_ShouldCreate_WhenClientAndUserHasValue()
		{
			//Arrange
			var receiptNumber = 1000;
			var clientNumber = 1;
			var user = "User";
			var rampLocationId = 1;
			var dateCreate = TestDates.UtcNow;
			//Act
			var receipt = Receipt.Create(receiptNumber, clientNumber, user, rampLocationId, dateCreate);
			//Assert
			Assert.NotNull(receipt);
			Assert.Equal(receiptNumber, receipt.ReceiptNumber);
			Assert.Equal(clientNumber, receipt.ClientId);
			Assert.Equal(user, receipt.PerformedBy);
			Assert.Equal(dateCreate, receipt.ReceiptDateTime);
			Assert.Equal(ReceiptStatus.Planned, receipt.ReceiptStatus);
			Assert.NotNull(receipt.Pallets);
			Assert.NotNull(receipt.DomainEvents);
			Assert.Empty(receipt.Pallets);
			Assert.Empty(receipt.DomainEvents);
		}
		[Fact]
		public void CreateReceipt_ThrowError_WhenClientHasNoValue()//int cannot be null, here is zero
		{
			//Arrange
			var receiptNumber = 1000;
			//var clientNumber = 1;
			var user = "User";
			var rampLocationId = 1;
			var dateCreate = TestDates.UtcNow;
			//Act&Assert
			var ex = Assert.Throws<ClientDomainException>(() =>
			Receipt.Create(receiptNumber, 0, user, rampLocationId, dateCreate));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void CreateReceipt_ThrowError_WhenUserHasNoValue(string? user)//int cannot be null, here is zero
		{
			//Arrange
			var receiptNumber = 1000;
			var clientNumber = 1;
			var rampLocationId = 1;
			var dateCreate = TestDates.UtcNow;
			//Act&Assert
			var ex = Assert.Throws<InvalidUserIdDomainException>(() =>
			Receipt.Create(receiptNumber, clientNumber, user!, rampLocationId, dateCreate));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void Delete_ShouldDelete_WhenStatusPlanned()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateNewReceipt();
			var user = "User";
			//Act
			receipt.Delete(user);
			//Assert
			Assert.Equal(ReceiptStatus.Deleted, receipt.ReceiptStatus);
			var receiptHistory = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(ReceiptStatus.Deleted, receiptHistory.ReceiptStatus);
		}
		[Theory]
		[InlineData(ReceiptStatus.InProgress)]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		[InlineData(ReceiptStatus.Verified)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Cancelled)]
		[InlineData(ReceiptStatus.Deleted)]
		public void Delete_ThrowError_WhenStatusNotAllowed(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var user = "User";
			//Act&Assert
			Assert.Equal(status, receipt.ReceiptStatus);
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() => receipt.Delete(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(receipt.DomainEvents);
		}
	}
}
