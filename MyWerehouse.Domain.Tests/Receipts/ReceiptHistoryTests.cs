using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receiving.Events;
using MyWerehouse.Domain.Receiving.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptHistoryTests
	{
		[Fact]
		public void AddHistory_ShouldAdd_WhenEmptyListOfPallets()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateNewReceipt();

			var user = "User";
			//Act
			receipt.AddHistory(user);

			//Assert
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Equal(user, historyReceipt.UserId);
			Assert.Equal(1, historyReceipt.ClientId);
			Assert.Empty(historyReceipt.DetailDtos);
		}
		[Fact]
		public void AddHistory_ShouldAdd()
		{
			//Arrange
			var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1000, 1, "user", TestDates.DaysAgo(1), ReceiptStatus.Verified, 1);
			var product1 =ReceiptTestData.CreateProduct("chairs", 1);
			var product2 = ReceiptTestData.CreateProduct("eggs", 1);
			var location1 = ReceiptTestData.CreateLocation(1);
			var snapShot1 = location1.ToSnapshot();
			var location2 = ReceiptTestData.CreateLocation(2);
			var snapShot2 = location2.ToSnapshot();
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
			receipt.AddHistory(user);
			receipt.DetachPallet(pallet1);
			receipt.DetachPallet(pallet2);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location2);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location1);
			//Assert
			var historyReceipt = Assert.Single(receipt.DomainEvents.OfType<AddHistoryReceiptNotification>());
			Assert.Empty(receipt.HistoryReceipt);
			Assert.Equal(user, historyReceipt.UserId);
			Assert.Equal(1, historyReceipt.ClientId);
			Assert.Equal(2, historyReceipt.DetailDtos.Count);	
			var details1 = historyReceipt.DetailDtos.First(x=>x.PalletId == pallet1.Id);
			Assert.Equal(location1.Id, details1.LocationId);
			Assert.Equal(snapShot1, details1.LocationSnapShot);
			var details2 = historyReceipt.DetailDtos.First(x=>x.PalletId == pallet2.Id);
			Assert.Equal(location2.Id, details2.LocationId);
			Assert.Equal(snapShot2, details2.LocationSnapShot);
		}
	}
}
