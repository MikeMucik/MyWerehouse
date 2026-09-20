using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
    public class PalletBestBeforeTests
    {
        [Fact]
        public void CalculateQuantityDelta_ShouldSumBatches_WhenProductHasDifferentBestBeforeDates()
        {
            //Arrange
            var productId = Guid.NewGuid();
            var firstBestBefore = new DateOnly(2027, 1, 10);
            var secondBestBefore = new DateOnly(2027, 2, 10);
            var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
                PalletStatus.Available, Guid.NewGuid(), null);
            pallet.AddProduct(productId, 10, TestDates.UtcNow, firstBestBefore);
            pallet.AddProduct(productId, 20, TestDates.UtcNow, secondBestBefore);

            var updatedProducts = new List<ProductOnPallet>
            {
                ProductOnPallet.Create(productId, pallet.Id, 15, TestDates.UtcNow, firstBestBefore),
                ProductOnPallet.Create(productId, pallet.Id, 25, TestDates.UtcNow, secondBestBefore)
            };
            //Act
            var change = Assert.Single(pallet.CalculateQuantityDelta(updatedProducts));
            //Assert
            Assert.Equal(productId, change.ProductId);
            Assert.Equal(10, change.Quantity);
        }

        [Fact]
        public void ReplaceProducts_ShouldSetAbsoluteQuantity_WhenProductAndBestBeforeAreUnchanged()
        {
			//Arrange
			var productId = Guid.NewGuid();
            var bestBefore = new DateOnly(2027, 1, 10);
            var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
                PalletStatus.Available, Guid.NewGuid(), null);
            pallet.AddProduct(productId, 10, TestDates.UtcNow, bestBefore);
			//Act
			pallet.ReplaceProducts([
                ProductOnPallet.Create(productId, pallet.Id, 15, TestDates.UtcNow, bestBefore)
            ]);
			//Assert
			var product = Assert.Single(pallet.ProductsOnPallet);
            Assert.Equal(15, product.Quantity);
            Assert.Equal(bestBefore, product.BestBefore);
        }

        [Fact]
        public void IsCorrectDate_ShouldThrow_WhenRequiredBestBeforeExistsAndProductHasNoDate()
        {
			//Arrange
			var productId = Guid.NewGuid();
            var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
                PalletStatus.Available, Guid.NewGuid(), null);
			//Act
			pallet.AddProduct(productId, 10, TestDates.UtcNow, null);
			//Assert
			Assert.Throws<NotCorrectDateBestBeforeDomainException>(
                () => pallet.IsCorrectDate(new DateOnly(2027, 1, 10)));
        }

        [Fact]
        public void GetProductOnPallet_ShouldIgnoreBestBefore_WhenRequirementHasNoDate()
        {
			//Arrange
			var productId = Guid.NewGuid();
            var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
                PalletStatus.Available, Guid.NewGuid(), null);
            pallet.AddProduct(productId, 10, TestDates.UtcNow, null);
			//Act
			var product = pallet.GetProductOnPallet(productId, null);
			//Assert
			Assert.Equal(productId, product.ProductId);
            Assert.Null(product.BestBefore);
        }

        [Fact]
        public void AddReversePickedProduct_ShouldAddProduct_WhenBothBestBeforeDatesAreNull()
        {
			//Arrange
			var productId = Guid.NewGuid();
            var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
                PalletStatus.Available, Guid.NewGuid(), null);
            pallet.AddProduct(productId, 5, TestDates.UtcNow, null);
			//Act
			var (RestQuantity, AddedQuantity) = pallet.AddReversePickedProduct(
                productId, null, 4, 10, "user", "location");
			//Assert
			Assert.Equal(0, RestQuantity);
            Assert.Equal(4, AddedQuantity);
            Assert.Equal(9, pallet.ProductsOnPallet.Single().Quantity);
        }
    }
}
