using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Receiving.ReceivingExceptions;
using TestSupport;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptUpdateTests
	{
		[Theory]
		[InlineData(ReceiptStatus.InProgress)]
		[InlineData(ReceiptStatus.PhysicallyCompleted)]
		public void CheckToUpDate_ShouldPass_WhenAllowedStatus(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			//Act
			receipt.CheckToUpdate();
		}
		[Theory]
		[InlineData(ReceiptStatus.Planned)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Deleted)]
		[InlineData(ReceiptStatus.Cancelled)]
		public void CheckToUpDate_ThrowError_WhenNotAllowedStatus(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			//Act
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() =>
			receipt.CheckToUpdate());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void CheckToUpDate_ThrowError_WhenStatusVerfied()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.Verified);		
			//Act
			var ex = Assert.Throws<ReceiptAlreadyVerifyDomainException>(() =>
			receipt.CheckToUpdate());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void UpdateReceipt_ShouldUpdateList_WhenStatusInProgres()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet();
			var user = "User1";
			var clientId = 2;
			var date = TestDates.Now;
			//Act
			receipt.UpdateReceipt(user, clientId, date);
			//Assert
			Assert.Equal(ReceiptStatus.InProgress, receipt.ReceiptStatus);
			Assert.Equal(user, receipt.PerformedBy);
			Assert.Equal(clientId, receipt.ClientId);
			Assert.Equal(date, receipt.ReceiptDateTime);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(ReceiptStatus.Correction, historyReceipt.ReceiptStatus);
		}
		[Fact]
		public void UpdateReceipt_ShouldUpdateList_WhenStatusPhysicallyCompleted()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.PhysicallyCompleted);
			var user = "User1";
			var clientId = 2;
			var date = TestDates.Now;
			//Act
			receipt.UpdateReceipt(user, clientId, date);
			//Assert
			Assert.Equal(ReceiptStatus.InProgress, receipt.ReceiptStatus);
			Assert.Equal(user, receipt.PerformedBy);
			Assert.Equal(clientId, receipt.ClientId);
			Assert.Equal(date, receipt.ReceiptDateTime);
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(ReceiptStatus.Correction, historyReceipt.ReceiptStatus);
		}
		[Theory]
		[InlineData(ReceiptStatus.Planned)]
		[InlineData(ReceiptStatus.Correction)]
		[InlineData(ReceiptStatus.Cancelled)]
		[InlineData(ReceiptStatus.Deleted)]
		public void UpdateReceipt_ThrowError_WhenStatusNotAllowed(ReceiptStatus status)
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(status);
			var user = "User1";
			var clientId = 2;
			var date = TestDates.Now;
			//Act
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() =>
			receipt.UpdateReceipt(user, clientId, date));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(receipt.DomainEvents);
			Assert.Equal(status, receipt.ReceiptStatus);
		}
		[Fact]
		public void UpdateReceipt_ThrowError_WhenStatusVerfied()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet(ReceiptStatus.Verified);
			var user = "User1";
			var clientId = 2;
			var date = TestDates.Now;
			//Act
			var ex = Assert.Throws<ReceiptAlreadyVerifyDomainException>(() =>
			receipt.UpdateReceipt(user, clientId, date));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(receipt.DomainEvents);
			Assert.Equal(ReceiptStatus.Verified, receipt.ReceiptStatus);
		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ReturnEmpty_WhenEmptyList()
		{
			//Arrangr
			var receipt = ReceiptTestData.CreateReceiptWithPallet();
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			//Act
			var result = receipt.ChangeValuesReturnNewPallets(pallets, user);
			//Assert
			Assert.NotNull(result);
			Assert.Empty(result);
			Assert.Empty(receipt.Pallets);
		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ShouldChangeQuantity_WhenOnlyChangeQuantity()
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.InProgress, 1);
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			//var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(2));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var paletDto1 = new ReceiptPalletUpdate(pallet1.Id, pallet1.PalletNumber,
				product1.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			var paletDto2 = new ReceiptPalletUpdate(pallet2.Id, pallet2.PalletNumber,
				product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			pallets.AddRange(paletDto1, paletDto2);
			//Act
			var result = receipt.ChangeValuesReturnNewPallets(pallets, user);
			//Assert
			Assert.Empty(result);
			Assert.Equal(PalletStatus.Receiving, pallet1.Status);
			Assert.Equal(paletDto1.BestBefore, pallet1.ProductsOnPallet.Single().BestBefore);
			Assert.Equal(paletDto1.DateAdded, pallet1.ProductsOnPallet.Single().DateAdded);
			Assert.Equal(PalletStatus.Receiving, pallet2.Status);
			Assert.Equal(paletDto2.BestBefore, pallet2.ProductsOnPallet.Single().BestBefore);
			Assert.Equal(paletDto2.DateAdded, pallet2.ProductsOnPallet.Single().DateAdded);
			Assert.Equal(paletDto1.Quantity, pallet1.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto1.ProductId, pallet1.ProductsOnPallet.Single().ProductId);
			Assert.Equal(paletDto2.Quantity, pallet2.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto2.ProductId, pallet2.ProductsOnPallet.Single().ProductId);
			Assert.Equal(2, receipt.Pallets.Count);
			var historyPallet1 = Assert.Single(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			var historyPallet2 = Assert.Single(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(20, historyPallet1.Details.Single().Quantity);
			Assert.Equal(20, historyPallet2.Details.Single().Quantity);
			Assert.Equal(ReasonForPallet.Correction, historyPallet1.ReasonMovement);
			Assert.Equal(ReasonForPallet.Correction, historyPallet2.ReasonMovement);
		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ShouldChangeQuantity_WhenChangeProduct()
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.InProgress, 1);
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			//var location2 = ReceiptTestData.CreateLocation(2);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(2));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var paletDto1 = new ReceiptPalletUpdate(pallet1.Id, pallet1.PalletNumber,
				product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			var paletDto2 = new ReceiptPalletUpdate(pallet2.Id, pallet2.PalletNumber,
				product1.Id, 30, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			pallets.AddRange(paletDto1, paletDto2);
			//Act
			var result = receipt.ChangeValuesReturnNewPallets(pallets, user);
			//Assert
			Assert.Empty(result);
			Assert.Equal(paletDto1.Quantity, pallet1.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto1.ProductId, pallet1.ProductsOnPallet.Single().ProductId);
			Assert.Equal(paletDto2.Quantity, pallet2.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto2.ProductId, pallet2.ProductsOnPallet.Single().ProductId);
			Assert.Equal(2, receipt.Pallets.Count);
			var historyPallet1 = Assert.Single(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			var historyPallet2 = Assert.Single(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(20, historyPallet1.Details.Single().Quantity);
			Assert.Equal(30, historyPallet2.Details.Single().Quantity);
			Assert.Equal(ReasonForPallet.Correction, historyPallet1.ReasonMovement);
			Assert.Equal(ReasonForPallet.Correction, historyPallet2.ReasonMovement);
		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ShouldChangeQuantityAndDeleteSecond_WhenOnlyChangeQuantity()
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.InProgress, 1);
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(2));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var paletDto1 = new ReceiptPalletUpdate(pallet1.Id, pallet1.PalletNumber,
				product1.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			//var paletDto2 = new ReceiptPalletUpdate(pallet2.Id, pallet2.PalletNumber,
			//	product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			pallets.AddRange(paletDto1);
			//Act
			var result = receipt.ChangeValuesReturnNewPallets(pallets, user);
			//Assert
			Assert.Empty(result);
			Assert.Equal(paletDto1.Quantity, pallet1.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto1.ProductId, pallet1.ProductsOnPallet.Single().ProductId);
			Assert.Equal(PalletStatus.Cancelled, pallet2.Status);
			Assert.Null(pallet2.ReceiptId);
			Assert.Single(receipt.Pallets);
			var historyPallet1 = Assert.Single(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			var historyPallet2 = Assert.Single(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(20, historyPallet1.Details.Single().Quantity);
			Assert.Equal(10, historyPallet2.Details.Single().Quantity);
			Assert.Equal(PalletStatus.Cancelled, historyPallet2.PalletStatus);
			Assert.Equal(ReasonForPallet.Correction, historyPallet1.ReasonMovement);
			Assert.Equal(ReasonForPallet.Correction,historyPallet2.ReasonMovement);
			Assert.Empty(receipt.DomainEvents);
			//var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());

		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ReturnList_WhenAddingOnePallet()
		{
			//Arrangr
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.InProgress, 1);
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, receipt.Id, null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var paletDto1 = new ReceiptPalletUpdate(pallet1.Id, pallet1.PalletNumber,
				product1.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			var paletDto2 = new ReceiptPalletUpdate(pallet2.Id, pallet2.PalletNumber,
				product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			var paletDto3 = new ReceiptPalletUpdate(Guid.Empty, null,
				product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			pallets.AddRange(paletDto1, paletDto2, paletDto3);
			//Act
			var result = receipt.ChangeValuesReturnNewPallets(pallets, user);
			//Assert
			Assert.Single(result);
			Assert.Equal(product2.Id, result.First().ProductId);
			Assert.Equal(paletDto3.Quantity, result.First().Quantity);
			Assert.Equal(paletDto3.BestBefore, result.First().BestBefore);
			Assert.Equal(paletDto1.Quantity, pallet1.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto1.ProductId, pallet1.ProductsOnPallet.Single().ProductId);
			Assert.Equal(paletDto2.Quantity, pallet2.ProductsOnPallet.Single().Quantity);
			Assert.Equal(paletDto2.ProductId, pallet2.ProductsOnPallet.Single().ProductId);
			Assert.Equal(2, receipt.Pallets.Count);
			var historyPallet1 = Assert.Single(pallet1.DomainEvents.OfType<PalletHistoryNotification>());
			var historyPallet2 = Assert.Single(pallet2.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(20, historyPallet1.Details.Single().Quantity);
			Assert.Equal(20, historyPallet2.Details.Single().Quantity);
			Assert.Equal(PalletStatus.Receiving, historyPallet2.PalletStatus);
			Assert.Equal(ReasonForPallet.Correction, historyPallet1.ReasonMovement);
			Assert.Equal(ReasonForPallet.Correction, historyPallet2.ReasonMovement);
		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ThrowError_WhenOneNotBelongs()
		{
			//Arrangr
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.InProgress, 1);
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, Guid.NewGuid(), null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			receipt.Pallets.Add(pallet1);
			//receipt.Pallets.Add(pallet2);
			var paletDto1 = new ReceiptPalletUpdate(pallet1.Id, pallet1.PalletNumber,
				product1.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			var paletDto2 = new ReceiptPalletUpdate(pallet2.Id, pallet2.PalletNumber,
				product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			pallets.AddRange(paletDto1, paletDto2);
			//Act&Assert
			var ex = Assert.Throws<PalletDoesNotBelongToReceiptDomainException>(() =>
			receipt.ChangeValuesReturnNewPallets(pallets, user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.NotEqual(paletDto1.Quantity, pallet1.ProductsOnPallet.Single().Quantity);
			Assert.NotEqual(paletDto2.Quantity, pallet2.ProductsOnPallet.Single().Quantity);
			//Assert.Equal()
		}
		[Fact]
		public void ChangeValuesReturnNewPallets_ThrowError_WhenOneAllowedStatus()
		{
			//Arrangr
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.Cancelled, 1);
			var user = "User";
			var pallets = new List<ReceiptPalletUpdate>();
			var product1 = ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var pallet1 = Pallet.CreateForTests("Q1000", TestDates.DaysAgo(10), location1.Id, PalletStatus.New, receipt.Id, null);
			var pallet2 = Pallet.CreateForTests("Q1001", TestDates.DaysAgo(1), location1.Id, PalletStatus.Receiving, Guid.NewGuid(), null);
			pallet1.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet2.AddProduct(product2.Id, 10, TestDates.DaysAgo(1), TestDates.DateBestBeforeInMonths(1));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			receipt.Pallets.Add(pallet1);
			receipt.Pallets.Add(pallet2);
			var paletDto1 = new ReceiptPalletUpdate(pallet1.Id, pallet1.PalletNumber,
				product1.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			var paletDto2 = new ReceiptPalletUpdate(pallet2.Id, pallet2.PalletNumber,
				product2.Id, 20, TestDates.Now, TestDates.DateBestBeforeInMonths(1));
			pallets.AddRange(paletDto1, paletDto2);
			//Act&Assert
			var ex = Assert.Throws<InvalidReceiptStateDomainException>(() =>
			receipt.ChangeValuesReturnNewPallets(pallets, user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.NotEqual(paletDto1.Quantity, pallet1.ProductsOnPallet.Single().Quantity);
			Assert.NotEqual(paletDto2.Quantity, pallet2.ProductsOnPallet.Single().Quantity);			
		}
	}
}
