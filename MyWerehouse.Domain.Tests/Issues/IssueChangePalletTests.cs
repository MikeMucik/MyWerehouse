using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Events;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueChangePalletTests
	{
		[Theory]
		[InlineData(IssueStatus.Pending)]
		[InlineData(IssueStatus.PickingShortage)]
		[InlineData(IssueStatus.InProgress)]
		public void ReplacePalletInIssue_ShouldChange_WhenRequiredBestBeforeExistsAndNewPalletHasDate(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			var product = IssueTestData.CreateProduct("name", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);

			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act
			issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore);
			//Assert
			Assert.Same(newPallet, Assert.Single(issue.Pallets));
			Assert.Equal(status, issue.IssueStatus);
			Assert.DoesNotContain(oldPallet, issue.Pallets);
			Assert.Equal(issue.Id, newPallet.IssueId);
			Assert.Null(oldPallet.IssueId);
			Assert.Equal(PalletStatus.LockedForIssue, newPallet.Status);
			Assert.Equal(PalletStatus.Available, oldPallet.Status);
			var historyIssue = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());
			Assert.Equal(IssueStatus.ChangingPallet, historyIssue.IssueStatus);
			var historyOldPallet = Assert.Single(oldPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(PalletStatus.Available, historyOldPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.Correction, historyOldPallet.ReasonMovement);
			var historyNewPallet = Assert.Single(newPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(PalletStatus.LockedForIssue, historyNewPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.ToLoad, historyNewPallet.ReasonMovement);
			Assert.Equal("user", issue.PerformedBy);
			Assert.Equal(issue.Id, historyIssue.IssueId);
			Assert.Equal("user", historyIssue.UserId);
			var historyPallet = Assert.Single(historyIssue.DetailDtos);
			Assert.Equal(newPallet.Id, historyPallet.PalletId);
			Assert.Equal(newPallet.PalletNumber, historyPallet.PalletNumber);
			Assert.Equal(newPallet.LocationId, historyPallet.LocationId);
			Assert.Equal(location2.ToSnapshot(), historyPallet.LocationSnapShot);
			Assert.DoesNotContain(historyIssue.DetailDtos, detail => detail.PalletId == oldPallet.Id);
			Assert.Equal(oldPallet.Id, historyOldPallet.PalletId);
			Assert.Equal("user", historyOldPallet.UserId);
			Assert.Equal(newPallet.Id, historyNewPallet.PalletId);
			Assert.Equal("user", historyNewPallet.UserId);
			Assert.Empty(issue.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(oldPallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(newPallet.DomainEvents.OfType<ChangeStockNotification>());
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldAndNewIsTheSame()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = oldPallet;
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<CannotReplaceTheSamePalletDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.LockedForIssue, newPallet.Status);
			Assert.Equal(issue.Id, newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
			Assert.Empty(oldPallet.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldChangeFromInStock_WhenRequiredBestBeforeExistsAndNewPalletHasDateStatusIssueConfirmed()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.ConfirmedToLoad);
			var product = IssueTestData.CreateProduct("name", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.ToIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				PalletStatus.InStock, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);

			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act
			issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore);
			//Assert
			Assert.Same(newPallet, Assert.Single(issue.Pallets));
			Assert.Equal(IssueStatus.ConfirmedToLoad, issue.IssueStatus);
			Assert.DoesNotContain(oldPallet, issue.Pallets);
			Assert.Equal(issue.Id, newPallet.IssueId);
			Assert.Null(oldPallet.IssueId);
			Assert.Equal(PalletStatus.ToIssue, newPallet.Status);
			Assert.Equal(PalletStatus.Available, oldPallet.Status);
			var historyIssue = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());
			Assert.Equal(IssueStatus.ChangingPallet, historyIssue.IssueStatus);
			var historyOldPallet = Assert.Single(oldPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(PalletStatus.Available, historyOldPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.Correction, historyOldPallet.ReasonMovement);
			var historyNewPallet =newPallet.DomainEvents.OfType<PalletHistoryNotification>().ToList();
			Assert.Equal(2, historyNewPallet.Count());
			Assert.Equal(PalletStatus.LockedForIssue, historyNewPallet[0].PalletStatus);
			Assert.Equal(PalletStatus.ToIssue, historyNewPallet[1].PalletStatus);
			Assert.Equal(ReasonForPallet.ToLoad, historyNewPallet[0].ReasonMovement);
			Assert.Equal(ReasonForPallet.ToLoad, historyNewPallet[1].ReasonMovement);
			Assert.Equal("user", issue.PerformedBy);
			Assert.Equal(issue.Id, historyIssue.IssueId);
			Assert.Equal("user", historyIssue.UserId);
			var historyPallet = Assert.Single(historyIssue.DetailDtos);
			Assert.Equal(newPallet.Id, historyPallet.PalletId);
			Assert.Equal(newPallet.PalletNumber, historyPallet.PalletNumber);
			Assert.Equal(newPallet.LocationId, historyPallet.LocationId);
			Assert.Equal(location2.ToSnapshot(), historyPallet.LocationSnapShot);
			Assert.DoesNotContain(historyIssue.DetailDtos, detail => detail.PalletId == oldPallet.Id);
			Assert.Equal(oldPallet.Id, historyOldPallet.PalletId);
			Assert.Equal("user", historyOldPallet.UserId);
			Assert.All(historyNewPallet, history =>
			{
				Assert.Equal(newPallet.Id, history.PalletId);
				Assert.Equal("user", history.UserId);
			});
			Assert.Empty(issue.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(oldPallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(newPallet.DomainEvents.OfType<ChangeStockNotification>());
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenRequiredBestBeforeExistsAndNewPalletHasNoDate()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<ProductOnPalletsAreNotTheSameBBDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Theory]
		[InlineData(IssueStatus.New)]
		[InlineData(IssueStatus.IsShipped)]
		[InlineData(IssueStatus.ChangingPallet)]
		[InlineData(IssueStatus.Archived)]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.RequiresCorrection)]
		public void ReplacePalletInIssue_ShouldThrow_WhenWrongStatus(IssueStatus status)
		{
			var issue = IssueTestData.CreateIssue(status);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(status, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletNotAssignToIssue()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), null);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<NotBelongsToIssueDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Null(oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletNotInIssue()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			//issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<NotBelongsToIssueDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Empty(issue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.Picking)]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.ReversePicking)]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletWrongStatus(PalletStatus status)
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				status, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(status, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletIsAlreadyLoaded()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.Loaded, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<PalletAlreadyLoadedDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.Loaded, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenNewPalletIsAssignedToOtherIssue()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var issue1 = IssueTestData.CreateIssue(IssueStatus.Pending);
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), issue1.Id);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<AlreadyAssignedDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Equal(issue1.Id, newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenPalletsHaveDiffrentProducts()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var product1 = IssueTestData.CreateProduct("name", 2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product1.Id, 10, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<ProductOnPalletsAreNotTheSameDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenPalletsHaveDiffrentQuantityProducts()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 8, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<ProductOnPalletsAreNotTheSameAmountDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletIsPickingPalletAndHasMoreThanOneProduct()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var product1 = IssueTestData.CreateProduct("name", 2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			oldPallet.AddProduct(product1.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 8, TestDates.UtcNow, null);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<NotOneProductsOnPalletDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenNewPalletIsPickingPalletAndHasMoreThanOneProduct()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var product1 = IssueTestData.CreateProduct("name", 2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);

			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 8, TestDates.UtcNow, null);
			newPallet.AddProduct(product1.Id, 10, TestDates.UtcNow, requiredBestBefore);
			issue.Pallets.Add(oldPallet);
			//Act&Assert
			var ex = Assert.Throws<NotOneProductsOnPalletDomainException>(() =>
					issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
		}
		[Theory]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.Loaded)]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.LockedForIssue)]
		[InlineData(PalletStatus.Picking)]
		[InlineData(PalletStatus.Cancelled)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.ToIssue)]
		public void ReplacePalletInIssue_ShouldThrow_WhenNewPalletWrongStatus(PalletStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				status, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
				issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(status, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
		}

		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenNewPalletBestBeforeIsEarlierThanRequired()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var previousUser = issue.PerformedBy;
			var product = IssueTestData.CreateProduct("name", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore.AddDays(-1));
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act&Assert
			var ex = Assert.Throws<ProductOnPalletsAreNotTheSameBBDomainException>(() =>
				issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(previousUser, issue.PerformedBy);
			Assert.Same(oldPallet, Assert.Single(issue.Pallets));
			Assert.Equal(PalletStatus.LockedForIssue, oldPallet.Status);
			Assert.Equal(issue.Id, oldPallet.IssueId);
			Assert.Equal(PalletStatus.Available, newPallet.Status);
			Assert.Null(newPallet.IssueId);
			Assert.Empty(issue.DomainEvents);
			Assert.Empty(oldPallet.DomainEvents);
			Assert.Empty(newPallet.DomainEvents);
		}

		[Fact]
		public void ReplacePalletInIssue_ShouldChange_WhenNewPalletBestBeforeIsLaterThanRequired()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var product = IssueTestData.CreateProduct("name", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var requiredBestBefore = TestDates.DateBestBeforeInMonths(1);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore.AddDays(1));
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act
			issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore);
			//Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal("user", issue.PerformedBy);
			Assert.Same(newPallet, Assert.Single(issue.Pallets));
			Assert.DoesNotContain(oldPallet, issue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, newPallet.Status);
			Assert.Equal(issue.Id, newPallet.IssueId);
			Assert.Equal(PalletStatus.Available, oldPallet.Status);
			Assert.Null(oldPallet.IssueId);
			var historyIssue = Assert.Single(issue.DomainEvents.OfType<AddHistoryIssueNotification>());
			Assert.Equal(issue.Id, historyIssue.IssueId);
			Assert.Equal(IssueStatus.ChangingPallet, historyIssue.IssueStatus);
			Assert.Equal("user", historyIssue.UserId);
			var historyPallet = Assert.Single(historyIssue.DetailDtos);
			Assert.Equal(newPallet.Id, historyPallet.PalletId);
			Assert.Equal(newPallet.PalletNumber, historyPallet.PalletNumber);
			Assert.Equal(newPallet.LocationId, historyPallet.LocationId);
			Assert.Equal(location2.ToSnapshot(), historyPallet.LocationSnapShot);
			Assert.DoesNotContain(historyIssue.DetailDtos, detail => detail.PalletId == oldPallet.Id);
			var historyOldPallet = Assert.Single(oldPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(oldPallet.Id, historyOldPallet.PalletId);
			Assert.Equal(PalletStatus.Available, historyOldPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.Correction, historyOldPallet.ReasonMovement);
			Assert.Equal("user", historyOldPallet.UserId);
			var historyNewPallet = Assert.Single(newPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(newPallet.Id, historyNewPallet.PalletId);
			Assert.Equal(PalletStatus.LockedForIssue, historyNewPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.ToLoad, historyNewPallet.ReasonMovement);
			Assert.Equal("user", historyNewPallet.UserId);
			Assert.Empty(issue.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(oldPallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(newPallet.DomainEvents.OfType<ChangeStockNotification>());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ReplacePalletInIssue_ShouldChange_WhenBestBeforeIsNotRequired(bool newPalletHasBestBefore)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
			var product = IssueTestData.CreateProduct("name", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var oldBestBefore = TestDates.DateBestBeforeInMonths(1);
			DateOnly? newBestBefore = newPalletHasBestBefore ? oldBestBefore.AddDays(-1) : null;
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issue.Id);
			oldPallet.AddProduct(product.Id, 10, TestDates.UtcNow, oldBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, newBestBefore);
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act
			issue.ReplacePalletInIssue(oldPallet, newPallet, "user", null);
			//Assert
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal("user", issue.PerformedBy);
			Assert.Same(newPallet, Assert.Single(issue.Pallets));
			Assert.DoesNotContain(oldPallet, issue.Pallets);
			Assert.Equal(PalletStatus.LockedForIssue, newPallet.Status);
			Assert.Equal(issue.Id, newPallet.IssueId);
			Assert.Equal(PalletStatus.Available, oldPallet.Status);
			Assert.Null(oldPallet.IssueId);
			var historyIssue = Assert.Single(issue.DomainEvents.OfType<AddHistoryIssueNotification>());
			Assert.Equal(issue.Id, historyIssue.IssueId);
			Assert.Equal(IssueStatus.ChangingPallet, historyIssue.IssueStatus);
			Assert.Equal("user", historyIssue.UserId);
			var historyPallet = Assert.Single(historyIssue.DetailDtos);
			Assert.Equal(newPallet.Id, historyPallet.PalletId);
			Assert.Equal(newPallet.PalletNumber, historyPallet.PalletNumber);
			Assert.Equal(newPallet.LocationId, historyPallet.LocationId);
			Assert.Equal(location2.ToSnapshot(), historyPallet.LocationSnapShot);
			Assert.DoesNotContain(historyIssue.DetailDtos, detail => detail.PalletId == oldPallet.Id);
			var historyOldPallet = Assert.Single(oldPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(oldPallet.Id, historyOldPallet.PalletId);
			Assert.Equal(PalletStatus.Available, historyOldPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.Correction, historyOldPallet.ReasonMovement);
			Assert.Equal("user", historyOldPallet.UserId);
			var historyNewPallet = Assert.Single(newPallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(newPallet.Id, historyNewPallet.PalletId);
			Assert.Equal(PalletStatus.LockedForIssue, historyNewPallet.PalletStatus);
			Assert.Equal(ReasonForPallet.ToLoad, historyNewPallet.ReasonMovement);
			Assert.Equal("user", historyNewPallet.UserId);
			Assert.Empty(issue.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(oldPallet.DomainEvents.OfType<ChangeStockNotification>());
			Assert.Empty(newPallet.DomainEvents.OfType<ChangeStockNotification>());
		}

		[Theory]
		[InlineData(IssueStatus.Cancelled)]
		[InlineData(IssueStatus.Archived)]
		public void ChangeStatus_ThrowError_WhenStatusNotAllowed(IssueStatus status)
		{
			//Arrange
			var issue = IssueTestData.CreateIssue(status);
			//Act&Assert
			var ex = Assert.Throws<NotAllowedOperationDomainException>(()=>
			issue.ChangeStatus(IssueStatus.Pending));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
	}
}
