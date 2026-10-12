using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletLocationTests
	{
		[Fact]
		public void MoveToLocation_ChangeStatusAndLocation_WhenStatusInStock()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(PalletStatus.InStock);
			var location1 = PalletTestData.CreateLocation(1);
			var snapShot1 = location1.ToSnapshot();
			var location2 = PalletTestData.CreateLocation(2);
			var snapShot2 = location2.ToSnapshot();
			var user = "User";
			//Act
			pallet.MoveToLocation(location2.Id, snapShot2, location1.Id, snapShot1, user);
			//Assert
			Assert.Equal(location2.Id, pallet.LocationId);
			Assert.Equal(PalletStatus.Available, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(Histories.Models.ReasonForPallet.Moved, historyPallet.ReasonMovement);
			Assert.Equal(location1.Id, historyPallet.SourceLocationId);
			Assert.Equal(location2.Id, historyPallet.DestinationLocationId);
		}
		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.ToIssue)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.LockedForIssue)]
		[InlineData(PalletStatus.Picking)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.New)]

		public void MoveToLocation_NoChangeStatusOnlyLocation_WhenStatusOk(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(status);
			var location1 = PalletTestData.CreateLocation(1);
			var snapShot1 = location1.ToSnapshot();
			var location2 = PalletTestData.CreateLocation(2);
			var snapShot2 = location2.ToSnapshot();
			var user = "User";
			//Act
			pallet.MoveToLocation(location2.Id, snapShot2, location1.Id, snapShot1, user);
			//Assert
			Assert.Equal(location2.Id, pallet.LocationId);
			Assert.Equal(status, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(Histories.Models.ReasonForPallet.Moved, historyPallet.ReasonMovement);
		}
		[Theory]
		[InlineData(PalletStatus.Loaded)]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.Archived)]
		public void MoveToLocation_ThrowError_WhenStatusNotAllowed(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(status);
			var location1 = PalletTestData.CreateLocation(1);
			var snapShot1 = location1.ToSnapshot();
			var location2 = PalletTestData.CreateLocation(2);
			var snapShot2 = location2.ToSnapshot();
			var user = "User";
			//Act
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.MoveToLocation(location2.Id, snapShot2, location1.Id, snapShot1, user));			
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents);
		}
	}
}
