using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletIssueAssignmentTests
	{
		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		public void ReserveToIssue_ChangeStatus_WhenStatusEntry(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act
			pallet.ReserveToIssue(issueId, user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.LockedForIssue, pallet.Status);
			Assert.Equal(issueId, pallet.IssueId);			
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ToLoad, historyPallet.ReasonMovement);
			Assert.Equal(PalletStatus.LockedForIssue, historyPallet.PalletStatus);
		}
		[Theory]
		[InlineData(PalletStatus.LockedForIssue)]
		[InlineData(PalletStatus.Picking)]
		public void ReserveToIssue_NoChangeStatus_WhenStatusEntry(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act
			pallet.ReserveToIssue(issueId, user, snapShot);
			//Assert
			Assert.Equal(status, pallet.Status);
			Assert.Equal(issueId, pallet.IssueId);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ToLoad, historyPallet.ReasonMovement);
			Assert.Equal(status, historyPallet.PalletStatus);
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Loaded)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.Receiving)]
		public void ReserveToIssue_ThrowError_WhenWrongStatus(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.ReserveToIssue(issueId, user, snapShot));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void ReserveToIssue_ThrowError_WhenStatusToIssue()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, PalletStatus.ToIssue);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<AlreadyAssignedDomainException>(() =>
			pallet.ReserveToIssue(issueId, user, snapShot));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void AssignToIssue_NoChangeStatus_WhenStatusToIssue()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, PalletStatus.ToIssue);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act
			pallet.AssignToIssue(issueId, user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.ToIssue, pallet.Status);
			Assert.Equal(issueId, pallet.IssueId);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ToLoad, historyPallet.ReasonMovement);
			Assert.Equal(PalletStatus.ToIssue, historyPallet.PalletStatus);
		}
		[Fact]
		public void AssignToIssue_ChangeStatus_WhenLockedForIssue()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, PalletStatus.LockedForIssue);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act
			pallet.AssignToIssue(issueId, user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.ToIssue, pallet.Status);
			Assert.Equal(issueId, pallet.IssueId);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ToLoad, historyPallet.ReasonMovement);
			Assert.Equal(PalletStatus.ToIssue, historyPallet.PalletStatus);
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Loaded)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.Picking)]
		public void AssignToIssue_ThrowError_WhenWrongStatus(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.AssignToIssue(issueId, user, snapShot));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(pallet.DomainEvents);
		}
		[Theory]
		[InlineData(PalletStatus.ToIssue)]
		[InlineData(PalletStatus.LockedForIssue)]
		public void DetachFromIssue_ChangeStatusSendDomainEvent(PalletStatus status)
		{
			{
				//Arrange
				var pallet = PalletTestData.CreatePalletForIssue("Q1000", 1, status);
				var location = PalletTestData.CreateLocation(1);
				var snapShot = location.ToSnapshot();
				var user = "User";
				//Act
				pallet.DetachFromIssue(user, snapShot, ReasonForPallet.CancelIssue);
				//Assert
				Assert.Equal(PalletStatus.Available, pallet.Status);
				Assert.Null(pallet.IssueId);
				var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
				Assert.Equal(ReasonForPallet.CancelIssue, historyPallet.ReasonMovement);
				Assert.Equal(PalletStatus.Available, historyPallet.PalletStatus);
			}
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.Picking)]
		public void DetachFromIssue_NoChangeStatusSendDomainEvent(PalletStatus status)
		{
			{
				//Arrange
				var pallet = PalletTestData.CreatePalletForIssue("Q1000", 1, status);
				var location = PalletTestData.CreateLocation(1);
				var snapShot = location.ToSnapshot();
				var user = "User";
				//Act
				pallet.DetachFromIssue(user, snapShot, Histories.Models.ReasonForPallet.CancelIssue);
				//Assert
				Assert.Equal(status, pallet.Status);
				Assert.Null(pallet.IssueId);
				var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
				Assert.Equal(Histories.Models.ReasonForPallet.CancelIssue, historyPallet.ReasonMovement);
			}
		}
		[Theory]
		[InlineData(PalletStatus.Loaded)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Cancelled)]
		public void DetachFromIssue_ThrowError(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletForIssue("Q1000", 1, status);
			var issueId = pallet.IssueId;
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			//Act
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.DetachFromIssue(user, snapShot, Histories.Models.ReasonForPallet.CancelIssue));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(issueId, pallet.IssueId);
			Assert.Equal(status, pallet.Status);
			Assert.Empty(pallet.DomainEvents);
		}

		[Fact]		
		public void MarkAsLoaded_ChangeStatusAddDomainEvent_WhenAllowedStatus()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletForIssue("Q1000", 1, PalletStatus.ToIssue);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			//Act
			pallet.MarkAsLoaded(user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.Loaded, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Loaded, historyPallet.ReasonMovement);
			Assert.Equal(PalletStatus.Loaded, historyPallet.PalletStatus);
		}

		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.Picking)]
		[InlineData(PalletStatus.LockedForIssue)]
		public void MarkAsLoaded_ThrowError_WhenWrongStatus(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletForIssue("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			//Act
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.MarkAsLoaded(user, snapShot));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void MarkAsLoaded_ThrowError_LoadedStatus()
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletForIssue("Q1000", 1, PalletStatus.Loaded);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var user = "User";
			//Act
			var ex = Assert.Throws<PalletAlreadyLoadedDomainException>(() =>
			pallet.MarkAsLoaded(user, snapShot));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
	}
}
