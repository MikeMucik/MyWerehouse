using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
    public class PalletBestBeforeTests
    {       
		[Fact]
		public void IsCorrectDate_ShouldNoThrow_WhenNotRequiredBestBefore()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);			
			pallet.AddProduct(productId, 10, TestDates.UtcNow, null);
			//Act
			pallet.IsCorrectDate(null);
		}
		[Fact]
		public void IsCorrectDate_ShouldNoThrow_WhenProductDateFuther()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var dateBBPallet = TestDates.DateBestBeforeInMonths(5);
			var dateBBNOrder = TestDates.DateBestBeforeInMonths(3);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);			
			pallet.AddProduct(productId, 10, TestDates.UtcNow, dateBBPallet);
			//Act
			pallet.IsCorrectDate(dateBBNOrder);
		}
		[Fact]
		public void IsCorrectDate_ShouldNoThrow_WhenTheSameDate()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var date = TestDates.DateBestBeforeInMonths(5);
			
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, date);
			//Act
			pallet.IsCorrectDate(date);
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
		public void IsCorrectDate_ShouldThrow_WhenBestBeforeOrderFuther()
		{
			//Arrange
			var productId = Guid.NewGuid();
			var dateBBPallet = TestDates.DateBestBeforeInMonths(4);
			var dateBBNOrder = TestDates.DateBestBeforeInMonths(6);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, 1,
				PalletStatus.Available, Guid.NewGuid(), null);
			pallet.AddProduct(productId, 10, TestDates.UtcNow, dateBBPallet);
			//Act&Assert
			Assert.Throws<NotCorrectDateBestBeforeDomainException>(
				() => pallet.IsCorrectDate(dateBBNOrder));
		}		
    }
}
