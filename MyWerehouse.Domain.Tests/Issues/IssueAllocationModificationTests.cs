using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pickings.Models;
using MyWerehouse.Domain.Products.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueAllocationModificationTests
	{
		[Fact]
		public void BeginAllocation_ShouldChangeStatus_WhenStatusNew()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.New);
			//Act
			issue.BeginAllocation();
			//Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
		}
		[Theory]
		[InlineData(IssueStatus.RequiresCorrection)]
		[InlineData(IssueStatus.Pending)]
		public void BeginAllocation_ShouldNotChangeStatus(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			//Act
			issue.BeginAllocation();
			//Assert
			Assert.Equal(status, issue.IssueStatus);
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		[InlineData(IssueStatus.PickingShortage)]
		public void BeginAllocation_ShouldThrow(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.BeginAllocation());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}

		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void MarkAllocationCompleted_ShouldMark_WhenStatusOk(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			var user = "User";
			//Act
			issue.MarkAllocationCompleted(user);
			//Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.NotNull(issue.HistoryIssues);
			var historyEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyEvent.IssueId);
			Assert.Equal(IssueStatus.Pending, historyEvent.IssueStatus);
			Assert.Equal(user, historyEvent.UserId);
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		[InlineData(IssueStatus.PickingShortage)]
		public void MarkAllocationCompleted_ShouldThrow_WhenStatusOk(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.MarkAllocationCompleted(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void MarkAllocationNotCompleted_ShouldMark_WhenStatusOk(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			var user = "User";
			//Act
			issue.MarkAllocationNotCompleted(user);
			//Assert
			Assert.Equal(IssueStatus.RequiresCorrection, issue.IssueStatus);
			Assert.NotNull(issue.HistoryIssues);
			var historyIssue = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());
			Assert.Equal(IssueStatus.RequiresCorrection, historyIssue.IssueStatus);
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		[InlineData(IssueStatus.PickingShortage)]
		public void MarkAllocationNotCompleted_ShouldThrow_WhenStatusOk(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.MarkAllocationNotCompleted(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void PrepareForReallocation_ReturnLists_WhenOnlyPalletsIncluded()
		{
			//Arrange			
			var client = IssueTestData.CreateClient();
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue();
			var product = IssueTestData.CreateProduct("Prod1", 1);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Available, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			//Act
			var result = issue.PrepareForReallocation(client.Id, "user", TestDates.TodayDateTime);
			//Assert
			Assert.Empty(issue.Pallets);
			Assert.Null(pallet1.IssueId);
			Assert.Null(pallet2.IssueId);
			Assert.Null(pallet2.IssueId);
			Assert.NotNull(result);
			Assert.NotNull(result.Pallets);
			Assert.NotNull(result.ListPalletsIds);
			Assert.Equal(2, result.Pallets.Count);
			Assert.Empty(result.ListPalletsIds);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet2.Status);
		}

		[Fact]
		public void PrepareForReallocation_ReturnLists_WhenPickingTasksExist()
		{
			//Arrange			
			var client2 = IssueTestData.CreateClient(2);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var location3 = IssueTestData.CreateLocation(3);
			var issue = IssueTestData.CreateIssue();
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var product2 = IssueTestData.CreateProduct("Prod2", 2);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.Available, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Available, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));

			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);

			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, location3.Id, PalletStatus.ToPicking, null, null);
			pallet3.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, pallet3.ProductsOnPallet.Single().Quantity,
				location3.Id, TestDates.DaysAgo(1));
			var pickingTask1 = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet1.Id, issue.Id, 5, PickingStatus.Allocated, product1.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask1, issue);
			issue.PickingTasks.Add(pickingTask1);

			var pallet4 = Pallet.CreateForTests("P3", TestDates.UtcNow, location3.Id, PalletStatus.ToPicking, null, null);
			pallet4.AddProduct(product2.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet4.Id, pallet4.ProductsOnPallet.Single().Quantity,
				location3.Id, TestDates.DaysAgo(1));
			var pickingTask2 = PickingTask.CreateForSeed(Guid.NewGuid(), virtualPallet2.Id, issue.Id, 5, PickingStatus.Allocated, product2.Id,
				null, null, null, 0);
			typeof(PickingTask).GetProperty(nameof(PickingTask.Issue))!
				.SetValue(pickingTask2, issue);
			issue.PickingTasks.Add(pickingTask2);
			//Act
			var result = issue.PrepareForReallocation(client2.Id, "user", TestDates.TodayDateTime);
			//Assert
			Assert.Empty(issue.Pallets);
			Assert.Empty(issue.PickingTasks);
			Assert.Equal(PickingStatus.Cancelled, pickingTask1.PickingStatus);
			Assert.Equal(PickingStatus.Cancelled, pickingTask2.PickingStatus);
			Assert.Equal(2, issue.ClientId);
			Assert.Null(pallet1.IssueId);
			Assert.Null(pallet2.IssueId);
			Assert.NotNull(result);
			Assert.NotNull(result.Pallets);
			Assert.NotNull(result.ListPalletsIds);
			Assert.Equal(2, result.Pallets.Count);
			Assert.Equal(2, result.ListPalletsIds.Count);
			Assert.Contains(virtualPallet1.Id, result.ListPalletsIds);
			Assert.Contains(virtualPallet2.Id, result.ListPalletsIds);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet2.Status);
		}
		[Fact]
		public void CompleteReallocation_ReturnTrue_WhenOk()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue();
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var reusablePallets = new List<Pallet>();
			reusablePallets.AddRange(pallet1, pallet2);
			var asignedPallets = new List<Pallet> { pallet1 };
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			//Act
			Issue.ReleaseUnusedPalletsToAvailable(asignedPallets, reusablePallets);
			//Assert
			Assert.Equal(PalletStatus.Available, pallet2.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
		}
		[Theory]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.InProgress)]
		public void StartEmergencypicking_ShouldNotThrowException_WhenStatusOk(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			//Act
			issue.StartEmergencyPicking();
			//Assert
			if (status == IssueStatus.New)
			{
				Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			}
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.RequiresCorrection)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		[InlineData(IssueStatus.PickingShortage)]
		public void StartEmergencypicking_ShouldThrowException_WhenBadStatus(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() =>
			issue.StartEmergencyPicking());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.InProgress)]
		public void CompletePickingPallet_ShouldChangeIssueStatus_WhenOk(IssueStatus status)
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var location = IssueTestData.CreateLocation(1);
			var issue = IssueTestData.CreateIssue(status);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, location.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet, location);
			var user = "user";
			//Act
			issue.CompletePickingPlanned(false, pallet, user);
			//Assert
			Assert.Equal(IssueStatus.PickingShortage, issue.IssueStatus);
			Assert.Equal(PalletStatus.OnHold, pallet.Status);
		}
		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.InProgress)]
		public void CompletePickingPallet_ShouldChangeIssueStatusAndHoldPallet_WhenOk(IssueStatus status)
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var location = IssueTestData.CreateLocation(1);
			var issue = IssueTestData.CreateIssue(status);
			var pallet = Pallet.CreateForTests("P1", TestDates.UtcNow, location.Id, PalletStatus.LockedForIssue, null, issue.Id);
			pallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet, location);
			var user = "user";
			//Act
			issue.CompletePickingPlanned(true, pallet, user);
			//Assert
			Assert.Equal(IssueStatus.InProgress, issue.IssueStatus);
			Assert.Equal(PalletStatus.OnHold, pallet.Status);
		}
		[Theory]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void DetremineModification_ReturnReallocationMode_WhenStatusToReallocation(IssueStatus status)
		{
			//Arrange
			var issue= IssueTestData.CreateIssue(status);
			//Act
			var result = issue.DetremineModificationMode();
			//Asssert
			Assert.Equal(IssueModificationMode.Reallocation, result);
		}
		[Fact]
		public void DetremineModification_ReturnSupplementeryIssueMode_WhenConfirmedToLoad()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.ConfirmedToLoad);
			//Act
			var result = issue.DetremineModificationMode();
			//Asssert
			Assert.Equal(IssueModificationMode.SupplementaryIssue, result);
		}
	
		[Theory]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.ChangingPallet)]
		public void DetremineModification_ThrowException_WhenWrongStatus(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			//Act&Asssert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(()=> issue.DetremineModificationMode());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
	}
}
