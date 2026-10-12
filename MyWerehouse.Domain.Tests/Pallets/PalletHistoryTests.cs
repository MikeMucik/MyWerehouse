using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletHistoryTests
	{
		[Fact]
		public void AddHistory_ShouldAddHistory()
		{
			//Arrange
			var pallet = PalletTestData.CreatePallet();
			var product1 = PalletTestData.CreateProduct("pencils", 3);
			var product2 = PalletTestData.CreateProduct("eggs", 1);
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			pallet.AddProduct(product1.Id, 10, TestDates.DaysAgo(1), null);
			pallet.AddProduct(product2.Id, 20, TestDates.DaysAgo(1), dateBB);
			//Act
			pallet.AddHistory(ReasonForPallet.Moved, "user", "location");
			var productOnPallet1 = pallet.ProductsOnPallet.Single(x=>x.ProductId == product1.Id);
			var productOnPallet2 = pallet.ProductsOnPallet.Single(x=>x.ProductId == product2.Id);
			productOnPallet1.SetQuantity(5); //Check change quantity after made domainEvent
			productOnPallet2.SetQuantity(0); //Check change quantity after made domainEvent
			//Assert
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Moved, historyPallet.ReasonMovement);

			Assert.Equal(pallet.Id, historyPallet.PalletId);
			Assert.Equal(pallet.PalletNumber, historyPallet.PalletNumber);
			Assert.Equal(pallet.Status, historyPallet.PalletStatus);
			Assert.Equal("user", historyPallet.UserId);
			var details1 = historyPallet.Details.Single(x=>x.ProductId == product1.Id);
			var details2 = historyPallet.Details.Single(x=>x.ProductId == product2.Id);
			Assert.Equal(10, details1.Quantity);
			Assert.Equal(20, details2.Quantity);
			Assert.Equal(dateBB, details2.BestBefore);
		}
	}
}
