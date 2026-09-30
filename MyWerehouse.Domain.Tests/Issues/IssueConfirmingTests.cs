using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueConfirmingTests
	{
		[Theory]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.Pending)]
		public void VerifyToLoad_ShouldAssignPallet_WhenStatusOk(IssueStatus status)
		{
			//Arrange
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
			//Act
			issue.VerifyToLoad(user);
			//Assert
			Assert.Equal(PalletStatus.ToIssue, pallet1.Status);
			Assert.Equal(PalletStatus.ToIssue, pallet2.Status);
			Assert.Equal(IssueStatus.ConfirmedToLoad, issue.IssueStatus);

			var historyIssueEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyIssueEvent.IssueId);
			Assert.Equal(IssueStatus.ConfirmedToLoad, historyIssueEvent.IssueStatus);
			Assert.Equal(user, historyIssueEvent.UserId);

			var historyPallet1Event = Assert.Single(pallet1.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet1.Id, historyPallet1Event.PalletId);
			Assert.Equal(PalletStatus.ToIssue, historyPallet1Event.PalletStatus);
			Assert.Equal(user, historyPallet1Event.UserId);

			var historyPallet2Event = Assert.Single(pallet2.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet2.Id, historyPallet2Event.PalletId);
			Assert.Equal(PalletStatus.ToIssue, historyPallet2Event.PalletStatus);
			Assert.Equal(user, historyPallet2Event.UserId);

			Assert.Equal(ReasonForPallet.ToLoad, historyPallet1Event.ReasonMovement);
			Assert.Equal(ReasonForPallet.ToLoad, historyPallet2Event.ReasonMovement);
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.New)]		
		[InlineData(IssueStatus.RequiresCorrection)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		public void VerifyToLoad_ReturnException_WhenStatusNotAllowed(IssueStatus status)
		{
			//Arrange
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
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.VerifyToLoad(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(PalletStatus.LockedForIssue, pallet1.Status);
			Assert.Equal(PalletStatus.LockedForIssue, pallet2.Status);
			Assert.Equal(status, issue.IssueStatus);

			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.DomainEvents);

			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}
		[Fact]
		public void CompletedLoad_ChangeStatus_WhenPalletsHaveStatusLoad()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(IssueStatus.InProgress);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.Loaded, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			//Act
			issue.CompletedLoad(user);
			//Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(user, issue.PerformedBy);
			var historyEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyEvent.IssueId);
			Assert.Equal(IssueStatus.IsShipped, historyEvent.IssueStatus);
			Assert.Equal(user, historyEvent.UserId);
		}
		[Fact]
		public void CompletedLoad_ThrowException_WhenPalletsHaveNoStatusLoad()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var status = IssueStatus.InProgress;
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.ToIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var previousUser = issue.PerformedBy;
			var user = "User";
			//Act&Assert
			var ex = Assert.ThrowsAny<NotEndedLoadingDomainException>(() => issue.CompletedLoad(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);

			Assert.Equal(PalletStatus.ToIssue, pallet1.Status);
			Assert.Equal(PalletStatus.Loaded, pallet2.Status);

			Assert.Empty(issue.DomainEvents);

			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}
		[Fact]
		public void ConfirmAfterLoading_ShouldChangeStatuses_WhenOk()
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var status = IssueStatus.IsShipped;
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.Loaded, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			//Act
			issue.ConfirmAfterLoading(user);
			//Assert
			Assert.Equal(IssueStatus.Archived, issue.IssueStatus);
			Assert.Equal(user, issue.PerformedBy);
			Assert.Equal(PalletStatus.Archived, pallet1.Status);
			Assert.Equal(PalletStatus.Archived, pallet2.Status);

			var historyIssueEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyIssueEvent.IssueId);
			Assert.Equal(IssueStatus.Archived, historyIssueEvent.IssueStatus);
			Assert.Equal(user, historyIssueEvent.UserId);

			var historyPallet1Event = Assert.Single(pallet1.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet1.Id, historyPallet1Event.PalletId);
			Assert.Equal(PalletStatus.Archived, historyPallet1Event.PalletStatus);
			Assert.Equal(user, historyPallet1Event.UserId);

			var historyPallet2Event = Assert.Single(pallet2.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet2.Id, historyPallet2Event.PalletId);
			Assert.Equal(PalletStatus.Archived, historyPallet2Event.PalletStatus);
			Assert.Equal(user, historyPallet2Event.UserId);

			Assert.Equal(ReasonForPallet.Loaded, historyPallet1Event.ReasonMovement);
			Assert.Equal(ReasonForPallet.Loaded, historyPallet2Event.ReasonMovement);

			var changeStockEvent = Assert.Single(issue.DomainEvents
				.OfType<ChangeStockNotification>());

			Assert.Equal(product1.Id, changeStockEvent.Changes.Single().ProductId);
			Assert.Equal(-20, changeStockEvent.Changes.Single().Quantity);
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.RequiresCorrection)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.ConfirmedToLoad)]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.Pending)]
		public void ConfirmAfterLoading_ReturnError_WhenWrongStatusIssue(IssueStatus status)
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.Loaded, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() => issue.ConfirmAfterLoading(user));

			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(PalletStatus.Loaded, pallet1.Status);
			Assert.Equal(PalletStatus.Loaded, pallet2.Status);
			Assert.Empty(issue.DomainEvents);

			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}
		[Fact]
		public void ConfirmAfterLoading_ReturnError_WhenWrongStatusPallet()
		{
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(IssueStatus.IsShipped);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.ToIssue, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotEndedLoadingDomainException>(() => issue.ConfirmAfterLoading(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(PalletStatus.ToIssue, pallet1.Status);
			Assert.Equal(PalletStatus.Loaded, pallet2.Status);

			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.DomainEvents);

			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
		}
		[Fact]
		public void FinishIssueNotCompleted_ShouldChangeIssueStatus_WhenStatusConfirmedToLoad()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(IssueStatus.ConfirmedToLoad);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.Loaded, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			//Act
			issue.FinishIssueNotCompleted(user);
			//Assert
			Assert.Equal(IssueStatus.IsShipped, issue.IssueStatus);
			Assert.Equal(user, issue.PerformedBy);
			Assert.Equal(PalletStatus.Loaded, pallet1.Status);
			Assert.Equal(PalletStatus.Loaded, pallet2.Status);
			var historyIssueEvent = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());

			Assert.Equal(issue.Id, historyIssueEvent.IssueId);
			Assert.Equal(IssueStatus.IsShipped, historyIssueEvent.IssueStatus);
			Assert.Equal(user, historyIssueEvent.UserId);

			var historyPallet1Event = Assert.Single(pallet1.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet1.Id, historyPallet1Event.PalletId);
			Assert.Equal(PalletStatus.Loaded, historyPallet1Event.PalletStatus);
			Assert.Equal(user, historyPallet1Event.UserId);

			var historyPallet2Event = Assert.Single(pallet2.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet2.Id, historyPallet2Event.PalletId);
			Assert.Equal(PalletStatus.Loaded, historyPallet2Event.PalletStatus);
			Assert.Equal(user, historyPallet2Event.UserId);

			Assert.Equal(ReasonForPallet.Loaded, historyPallet1Event.ReasonMovement);
			Assert.Equal(ReasonForPallet.Loaded, historyPallet2Event.ReasonMovement);
		}
		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.InProgress)]
		[InlineData(IssueStatus.RequiresCorrection)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.Pending)]
		public void FinishIssueNotCompleted_ReturnError_WhenStatusIsNotConfirmedToLoad(IssueStatus status)
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue(status);
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.Loaded, null, issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.Loaded, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "User";
			var previousUser = issue.PerformedBy;
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(()=>issue.FinishIssueNotCompleted(user));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(issue.DomainEvents);

			Assert.Empty(pallet1.DomainEvents);
			Assert.Empty(pallet2.DomainEvents);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Equal(status, issue.IssueStatus);
		}
		[Fact]
		public void CompareGoods_ReturnIsMatching_WhenAmountOnPalletsIsOK()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow,requiredBestBefore);
			var pallets = new List<Pallet>();
			pallets.AddRange(pallet1, pallet2);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 20, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(20, result.PreparedQuantity);
			Assert.Equal(20, result.OrderedQuantity);
			Assert.Equal(requiredBestBefore, result.BestBefore);
		}
		[Fact]
		public void CompareGoods_ReturnIsNoMatching_WhenPalletHasNoBB()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallets = new List<Pallet>();
			pallets.AddRange(pallet1, pallet2);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 20, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(10, result.PreparedQuantity);
			Assert.Equal(20, result.OrderedQuantity);
			Assert.Equal(requiredBestBefore, result.BestBefore);
		}
		[Fact]
		public void CompareGoods_ReturnIsMatching_WhenPalletHasBBButIssueItemNo()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallets = new List<Pallet>();
			pallets.AddRange(pallet1, pallet2);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 20, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(20, result.PreparedQuantity);
			Assert.Equal(20, result.OrderedQuantity);
			Assert.Equal(requiredBestBefore, result.BestBefore);
		}
		[Fact]
		public void CompareGoods_ReturnIsMatching_WhenAmountOnPalletsIsOKAndPickingPallets()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 5, TestDates.UtcNow, requiredBestBefore);
			
			var pallets = new List<Pallet>();
			
			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 25, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(25, result.PreparedQuantity);
			Assert.Equal(25, result.OrderedQuantity);
			Assert.Equal(requiredBestBefore, result.BestBefore);
		}
		[Fact]
		public void CompareGoods_ReturnIsNotMatching_WhenDatesAreNotCorrect()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var notrequiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(356));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, notrequiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 5, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 25, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(15, result.PreparedQuantity);
			Assert.Equal(25, result.OrderedQuantity);
			Assert.Equal(requiredBestBefore, result.BestBefore);
		}
		[Fact]
		public void CompareGoods_ReturnIsNotMatching_WhenQuantityAreNotCorrect()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 6, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 25, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(26, result.PreparedQuantity);
			Assert.Equal(25, result.OrderedQuantity);
			Assert.Equal(requiredBestBefore, result.BestBefore);
		}
		[Fact]
		public void CompareGoods_ThrowException_WhenPickingPalletIsNotClosed()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue();
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));

			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.Picking, null, issue.Id);
			pallet3.AddProduct(product.Id, 6, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 25, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var ex = Assert.Throws<PalletsNotReadyToLoadDomainException>(()=>
			issue.CompareGoods(product.Id));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void CompareGoods_ReturnIsNoMatchingAndIsConditional_WhenAmountOnPalletsIsOKAndPickingPalletsIsNotCorrect()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue(IssueStatus.PickingShortage);
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 3, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 25, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsMatching);
			Assert.True(result.IsConditional);
			Assert.Equal(23, result.PreparedQuantity);
			Assert.Equal(25, result.OrderedQuantity);
		}
		[Fact]
		public void CompareGoods_ReturnIsMatching_WhenShouldSkipAnotherProduct()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var product1 = IssueTestData.CreateProduct("pen", 2);
			var issue = IssueTestData.CreateIssue(IssueStatus.InProgress);
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 5, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 15, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(15, result.PreparedQuantity);
			Assert.Equal(15, result.OrderedQuantity);
		}
		[Fact]
		public void CompareGoods_ReturnIsMatching_WhenShouldCheckTwoProduct()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var product1 = IssueTestData.CreateProduct("pen", 2);
			var issue = IssueTestData.CreateIssue(IssueStatus.InProgress);
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 3, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 13, requiredBestBefore, TestDates.DaysAgo(1));
			var issueItem1 = IssueItem.CreateForSeed(1, issue.Id,
				product1.Id, 10, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.AddRange(issueItem,issueItem1);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			var result1 = issue.CompareGoods(product1.Id);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result1);
			Assert.True(result.IsMatching);
			Assert.True(result1.IsMatching);
			Assert.False(result.IsConditional);
			Assert.False(result1.IsConditional);
			Assert.Equal(13, result.PreparedQuantity);
			Assert.Equal(10, result1.PreparedQuantity);
			Assert.Equal(13, result.OrderedQuantity);
			Assert.Equal(10, result1.OrderedQuantity);
		}
		[Fact]
		public void CompareGoods_ReturnIsNoMatchingAndIsNoConditional_WhenAmountOnPalletsIsAndPickingPalletsIsTooMany()
		{
			//Arrange
			var product = IssueTestData.CreateProduct("pencil", 1);
			var issue = IssueTestData.CreateIssue(IssueStatus.PickingShortage);
			var requiredBestBefore = DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366));
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, 1, PalletStatus.LockedForIssue, null, issue.Id);
			pallet1.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, 2, PalletStatus.LockedForIssue, null, issue.Id);
			pallet2.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var pallet3 = Pallet.CreateForTests("P3", TestDates.UtcNow, 3, PalletStatus.ToIssue, null, issue.Id);
			pallet3.AddProduct(product.Id, 5, TestDates.UtcNow, requiredBestBefore);

			var pallets = new List<Pallet>();

			pallets.AddRange(pallet1, pallet2, pallet3);
			typeof(Issue).GetProperty(nameof(Issue.Pallets))!
				.SetValue(issue, pallets);
			var issueItem = IssueItem.CreateForSeed(1, issue.Id,
				product.Id, 23, requiredBestBefore, TestDates.DaysAgo(1));
			var listOfIssueItems = new List<IssueItem>();
			listOfIssueItems.Add(issueItem);
			typeof(Issue).GetProperty(nameof(Issue.IssueItems))!
				.SetValue(issue, listOfIssueItems);
			//Act
			var result = issue.CompareGoods(product.Id);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsMatching);
			Assert.False(result.IsConditional);
			Assert.Equal(25, result.PreparedQuantity);
			Assert.Equal(23, result.OrderedQuantity);
		}
	}
}
