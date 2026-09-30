using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pickings.Events;
using MyWerehouse.Domain.Pickings.Models;
using MyWerehouse.Domain.Warehouse.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueCancelTests
	{
		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void CancelIssueForDelete_ShouldCancellAllStaff_WhenStatusOk(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var location3 = IssueTestData.CreateLocation(3);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var pallet3 = Pallet.CreateForTests("P2", TestDates.UtcNow, location3.Id, PalletStatus.ToPicking, null, null);
			pallet3.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, 10, location3.Id, TestDates.DaysAgo(3));
			var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 3, PickingStatus.Allocated, product1.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask, issue);
			issue.PickingTasks.Add(pickingTask);
			var user = "User";
			var date = TestDates.UtcNow;
			//Act
			issue.CancelIssueForDelete(user, date);
			//Assert
			Assert.Equal(IssueStatus.Cancelled, issue.IssueStatus);
			Assert.Equal(PalletStatus.Available, pallet1.Status);
			Assert.Equal(PalletStatus.Available, pallet2.Status);

			var historyIssueEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyIssueEvent.IssueId);
			Assert.Equal(IssueStatus.Cancelled, historyIssueEvent.IssueStatus);
			Assert.Equal(user, historyIssueEvent.UserId);

			var historyPallet1Event = Assert.Single(pallet1.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet1.Id, historyPallet1Event.PalletId);
			Assert.Equal(PalletStatus.Available, historyPallet1Event.PalletStatus);
			Assert.Equal(user, historyPallet1Event.UserId);

			var historyPallet2Event = Assert.Single(pallet2.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet2.Id, historyPallet2Event.PalletId);
			Assert.Equal(PalletStatus.Available, historyPallet2Event.PalletStatus);
			Assert.Equal(user, historyPallet2Event.UserId);

			Assert.Equal(ReasonForPallet.CancelIssue, historyPallet1Event.ReasonMovement);
			Assert.Equal(ReasonForPallet.CancelIssue, historyPallet2Event.ReasonMovement);

			Assert.Equal(PickingStatus.Cancelled, pickingTask.PickingStatus);
			Assert.Equal(0, pickingTask.RequestedQuantity);
			Assert.Null(pallet1.IssueId);
			Assert.Null(pallet2.IssueId);

			var historyPickingTask = Assert.Single(pickingTask.DomainEvents
				.OfType<CreateHistoryPickingNotification>());

			Assert.Equal(pickingTask.Id, historyPickingTask.PickingTaskId);
			Assert.Equal(PickingStatus.Cancelled, historyPickingTask.StatusAfter);
			Assert.Equal(user, historyPickingTask.PerformedBy);

			
		}

		[Theory]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.Cancelled)]
		public void CancelIssue_ReturnError_WhenStatusNotAllowed(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			var date = TestDates.UtcNow;
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.CancelIssueForDelete(user, date));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet2.Status);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.DomainEvents);
			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}

		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void EnsuredCanBeDeleted_NoError_WhenPickingTasksPlanned(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var location3 = IssueTestData.CreateLocation(3);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var pallet3 = Pallet.CreateForTests("P2", TestDates.UtcNow, location3.Id, PalletStatus.ToPicking, null, null);
			pallet3.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, 10, location3.Id, TestDates.DaysAgo(3));
			var pickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 3, PickingStatus.Allocated, product1.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask, issue);
			issue.PickingTasks.Add(pickingTask);
			//Act&Assert
			issue.EnsureCanBeDeleted();
		}

		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void EnsuredCanBeDeleted_ReturnError_WhenPickingTaskChangedToHandPicking(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			//var location3 = IssueTestData.CreateLocation(3);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			//var date = TestDates.UtcNow;
			var pickingTask1 = PickingTask.CreateForSeed(Guid.NewGuid(), null, issue.Id, 5, PickingStatus.Available, product1.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask1, issue);
			issue.PickingTasks.Add(pickingTask1);
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.EnsureCanBeDeleted());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet2.Status);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.DomainEvents);
			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}

		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void EnsuredCanBeDeleted_ReturnError_WhenOneOfTwoPickingTasksChangedToHandPicking(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var location3 = IssueTestData.CreateLocation(3);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var pallet3 = Pallet.CreateForTests("P2", TestDates.UtcNow, location3.Id, PalletStatus.ToPicking, null, null);
			pallet3.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, 10, location3.Id, TestDates.DaysAgo(3));
			//var date = TestDates.UtcNow;
			var pickingTask1 = PickingTask.CreateForSeed(Guid.NewGuid(), null, issue.Id, 5, PickingStatus.Available, product1.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask1, issue);
			var pickingTask2 = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet.Id, issue.Id, 3, PickingStatus.Allocated, product1.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask2, issue);
			issue.PickingTasks.Add(pickingTask1);
			issue.PickingTasks.Add(pickingTask2);
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.EnsureCanBeDeleted());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet2.Status);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.DomainEvents);
			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}

		[Theory]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.RequiresCorrection)]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		public void Cancel_ShouldCancellAllStaff_WhenStatusOk(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, Guid.NewGuid(), null);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, null);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			//Act
			issue.Cancel(user);
			//Assert
			Assert.Equal(IssueStatus.Cancelled, issue.IssueStatus);
			Assert.Equal(user, issue.PerformedBy);
			var historyIssueEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyIssueEvent.IssueId);
			Assert.Equal(IssueStatus.Cancelled, historyIssueEvent.IssueStatus);
			Assert.Equal(user, historyIssueEvent.UserId);

			Assert.DoesNotContain(pallet1, issue.Pallets);
			Assert.Contains(pallet2, issue.Pallets);
		}
		[Theory]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.IsShipped)]
		public void EnsureCanBeCancelled_ReturnError_WhenInvalidStatus(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			var previousUser = issue.PerformedBy;
			//Act&Arrange
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.EnsureCanBeCancelled());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void Cancel_ShouldPreservePalletsInHistory_WhenDetachedReceiptPalletIsRemoved()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue();
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id,
				PalletStatus.ToIssue, Guid.NewGuid(), issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id,
				PalletStatus.ToIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 5, TestDates.UtcNow, null);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			//Act
			issue.DetachPallets(user);
			issue.Cancel(user);
			//Assert
			Assert.Equal(IssueStatus.Cancelled, issue.IssueStatus);
			Assert.Null(pallet1.IssueId);
			Assert.Equal(PalletStatus.Available, pallet1.Status);
			Assert.Same(pallet2, Assert.Single(issue.Pallets));
			Assert.Equal(issue.Id, pallet2.IssueId);
			Assert.Equal(PalletStatus.ToIssue, pallet2.Status);
			var historyEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());
			Assert.Equal(issue.Id, historyEvent.IssueId);
			Assert.Equal(IssueStatus.Cancelled, historyEvent.IssueStatus);
			Assert.Equal(user, historyEvent.UserId);
			Assert.Equal(2, historyEvent.DetailDtos.Count);
			Assert.Contains(historyEvent.DetailDtos, detail => detail.PalletId == pallet1.Id);
			Assert.Contains(historyEvent.DetailDtos, detail => detail.PalletId == pallet2.Id);
		}
	}
}
