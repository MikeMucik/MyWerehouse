using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletBaseCommandTests
	{
		[Fact]
		public void Create_ShouldCreate()
		{
			//Arrange			
			var location = PalletTestData.CreateLocation(1);
			var palletNumber = "Q1000";
			var time = TestDates.TodayDateTime;
			//Act
			var pallet = Pallet.Create(palletNumber, location.Id, time);
			//Assert
			Assert.NotNull(pallet);
			Assert.IsType<Guid>(pallet.Id);
			Assert.NotEqual(Guid.Empty ,pallet.Id);
			Assert.Equal(palletNumber, pallet.PalletNumber);
			Assert.Equal(location.Id, pallet.LocationId);
			Assert.Empty(pallet.ProductsOnPallet);
			Assert.Null(pallet.ReceiptId);
			Assert.Null(pallet.IssueId);
			Assert.Equal(PalletStatus.New, pallet.Status);
			Assert.Equal(time, pallet.DateReceived);
		}
		[Theory]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.Archived)]
		public void ChangeStatus_ThrowException_WhenNotAllowedStatus(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q9000", 1, status);
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() => pallet.ChangeStatus(PalletStatus.Available));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Theory]
		[InlineData(PalletStatus.Available)]//1
		[InlineData(PalletStatus.ToIssue)]//2
		[InlineData(PalletStatus.Damaged)]//3
		[InlineData(PalletStatus.OnHold)]//4
		[InlineData(PalletStatus.Loaded)]//5
		[InlineData(PalletStatus.ToPicking)]//6
		[InlineData(PalletStatus.Receiving)]//7
		[InlineData(PalletStatus.InStock)]//8
		[InlineData(PalletStatus.LockedForIssue)]//9
		[InlineData(PalletStatus.Picking)]//10
		[InlineData(PalletStatus.ReversePicking)]//11
		[InlineData(PalletStatus.New)]//12
		public void ChangeStatus_ShouldChnageStatus_WhenAllowedStatus(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q9000", 1, status);
			//Act
			pallet.ChangeStatus(PalletStatus.Cancelled);
			//Assert
			Assert.Equal(PalletStatus.Cancelled, pallet.Status);
		}
	}
}
