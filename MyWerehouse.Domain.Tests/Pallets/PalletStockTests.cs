using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Test;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletStockTests
	{
		[Fact]
		public void CalculateQuantityDelta_ShouldAddSumBatches_WhenProductHasDifferentBestBeforeDates()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);			
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
			pallet.AddProduct(productId, 20, TestDates.UtcNow, secondBestBefore);

			var updatedProducts = new List<ProductOnPallet>
			{
				ProductOnPallet.Create(productId, pallet.Id, 15, TestDates.UtcNow, firstBestBefore),
				ProductOnPallet.Create(productId, pallet.Id, 25, TestDates.UtcNow, secondBestBefore)
			};
			//Act&Assert
			var change = Assert.Single(pallet.CalculateQuantityDelta(updatedProducts));
			
			Assert.Equal(productId, change.ProductId);
			Assert.Equal(10, change.Quantity);
		}
		[Fact]
		public void CalculateQuantityDelta_ShouldAddNewAndDeleteOld_WhenTwoProductDifferent()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var productId1 = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);

			var updatedProducts = new List<ProductOnPallet>
			{
				ProductOnPallet.Create(productId1, pallet.Id, 25, TestDates.UtcNow, secondBestBefore)
			};
			//Act&Assert
			var change = pallet.CalculateQuantityDelta(updatedProducts);
			Assert.Equal(2, change.Count);
			//Assert.Equal(productId, change.ProductId);
			Assert.Equal(-10, change.Single(x=>x.ProductId == productId).Quantity);
			Assert.Equal(25, change.Single(x=>x.ProductId == productId1).Quantity);
		}

		[Fact]
		public void CalculateQuantityDelta_ShouldOddSumBatches_WhenProductHasDifferentBestBeforeDates()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
			pallet.AddProduct(productId, 20, TestDates.UtcNow, secondBestBefore);

			var updatedProducts = new List<ProductOnPallet>
			{
				ProductOnPallet.Create(productId, pallet.Id, 5, TestDates.UtcNow, firstBestBefore),
				ProductOnPallet.Create(productId, pallet.Id, 5, TestDates.UtcNow, secondBestBefore)
			};
			//Act&Assert
			var change = Assert.Single(pallet.CalculateQuantityDelta(updatedProducts));

			Assert.Equal(productId, change.ProductId);
			Assert.Equal(-20, change.Quantity);
		}
		[Fact]
		public void CalculateQuantityDelta_ShouldReturnZero_WhenProductChangedOnlyDifferentBestBeforeDates()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
			pallet.AddProduct(productId, 20, TestDates.UtcNow, secondBestBefore);

			var updatedProducts = new List<ProductOnPallet>
			{
				ProductOnPallet.Create(productId, pallet.Id, 25, TestDates.UtcNow, firstBestBefore),
				ProductOnPallet.Create(productId, pallet.Id, 5, TestDates.UtcNow, secondBestBefore)
			};
			//Act&Assert
			Assert.Empty(pallet.CalculateQuantityDelta(updatedProducts));
		}
		[Fact]
		public void AssignToWarehouse_ChangeStatusAndSendDomainEvent_WhenProductWithTwoBB()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
			pallet.AddProduct(productId, 20, TestDates.UtcNow, secondBestBefore);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			//Act
			pallet.AssignToWarehouse(snapShot, user);
			//Assert
			Assert.Equal(PalletStatus.InStock, pallet.Status);
			var hisotryPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(pallet.Id, hisotryPallet.PalletId);
			Assert.Equal(ReasonForPallet.New, hisotryPallet.ReasonMovement);
			var stockChange = Assert.Single(pallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Equal(30, stockChange.Changes.First(x => x.ProductId == productId).Quantity);
		}
		[Fact]
		public void AssignToWarehouse_ChangeStatusAndSendDomainEvent_WhenTwoProducts()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var productId1 = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
			pallet.AddProduct(productId1, 20, TestDates.UtcNow, secondBestBefore);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			//Act
			pallet.AssignToWarehouse(snapShot, user);
			//Assert
			Assert.Equal(PalletStatus.InStock, pallet.Status);
			var hisotryPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(pallet.Id, hisotryPallet.PalletId);
			Assert.Equal(ReasonForPallet.New, hisotryPallet.ReasonMovement);
			var stockChange = Assert.Single(pallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Equal(10, stockChange.Changes.First(x => x.ProductId == productId).Quantity);
			Assert.Equal(20, stockChange.Changes.First(x => x.ProductId == productId1).Quantity);
		}
		[Fact]
		public void Update_ShouldAddFullQuantityToStock_WhenPalletIsNew()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var bestBefore = TestDates.DateBestBeforeInMonths(5);
			var pallet = PalletTestData.CreatePalletWithStatus(PalletStatus.New);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, bestBefore);
			var snapShot = PalletTestData.CreateLocation(1).ToSnapshot();
			var user = "User";
			var updatedProducts = new List<ProductOnPallet>
			{
				ProductOnPallet.Create(productId, pallet.Id, 15, TestDates.UtcNow, bestBefore)
			};
			//Act
			pallet.Update(user, updatedProducts, PalletStatus.InStock, snapShot);
			//Assert
			Assert.Equal(PalletStatus.InStock, pallet.Status);
			var product = Assert.Single(pallet.ProductsOnPallet);
			Assert.Equal(productId, product.ProductId);
			Assert.Equal(15, product.Quantity);
			Assert.Equal(bestBefore, product.BestBefore);
			var history = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(pallet.Id, history.PalletId);
			Assert.Equal(user, history.UserId);
			Assert.Equal(ReasonForPallet.Correction, history.ReasonMovement);
			Assert.Equal(PalletStatus.InStock, history.PalletStatus);
			var stockChange = Assert.Single(pallet.DomainEvents.OfType<ChangeStockNotification>());
			var change = Assert.Single(stockChange.Changes);
			Assert.Equal(productId, change.ProductId);
			Assert.Equal(15, change.Quantity);
		}
		[Fact]
		public void Update_ChangeStatusesAndSendDomainEvents()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var productId1 = Guid.NewGuid();
			var firstBestBefore = TestDates.DateBestBeforeInMonths(5);
			var secondBestBefore = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
			pallet.AddProduct(productId1, 20, TestDates.UtcNow, secondBestBefore);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			var list = new List<ProductOnPallet>
			{
				ProductOnPallet.Create(productId, pallet.Id, 15, TestDates.Now, secondBestBefore),
				ProductOnPallet.Create(productId1, pallet.Id, 15, TestDates.Now, firstBestBefore)
			};
			//Act
			pallet.Update(user, list, PalletStatus.InStock, snapShot);
			//Assert
			Assert.Equal(PalletStatus.InStock, pallet.Status);
			var hisotryPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(pallet.Id, hisotryPallet.PalletId);
			Assert.Equal(15, pallet.ProductsOnPallet.First(x => x.ProductId == productId).Quantity);
			Assert.Equal(15, pallet.ProductsOnPallet.First(x => x.ProductId == productId1).Quantity);
			Assert.Equal(secondBestBefore, pallet.ProductsOnPallet.First(x => x.ProductId == productId).BestBefore);
			Assert.Equal(firstBestBefore, pallet.ProductsOnPallet.First(x => x.ProductId == productId1).BestBefore);
			Assert.Equal(ReasonForPallet.Correction, hisotryPallet.ReasonMovement);
			var stockChange = Assert.Single(pallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Equal(2, stockChange.Changes.Count());
			Assert.Equal(2, pallet.ProductsOnPallet.Count);	
			Assert.Equal(5, stockChange.Changes.First(x => x.ProductId == productId).Quantity);
			Assert.Equal(-5, stockChange.Changes.First(x => x.ProductId == productId1).Quantity);
		}
	}
}
