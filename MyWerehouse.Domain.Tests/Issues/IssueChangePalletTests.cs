using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueBestBeforeTests
	{
		[Fact]
		public void ReplacePalletInIssue_ShouldChange_WhenRequiredBestBeforeExistsAndNewPalletHasDate()
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
			newPallet.AddProduct(product.Id, 10, TestDates.UtcNow, requiredBestBefore);
			issue.Pallets.Add(oldPallet);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(oldPallet, location1);

			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(newPallet, location2);
			//Act
			issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore);
			//Assert
			Assert.Contains(newPallet, issue.Pallets);
			Assert.DoesNotContain(oldPallet, issue.Pallets);
			Assert.Equal(issue.Id, newPallet.IssueId);
			Assert.Null(oldPallet.IssueId);
			Assert.Equal(PalletStatus.ToIssue, newPallet.Status);
			Assert.Equal(PalletStatus.Available, oldPallet.Status);
			var historyIssue = Assert.Single(issue.DomainEvents
				.OfType<AddHistoryIssueNotification>());
			Assert.Equal(IssueStatus.ChangingPallet, historyIssue.IssueStatus);
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenRequiredBestBeforeExistsAndNewPalletHasNoDate()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldAndNewIsTheSame()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletNotAssignToIssue()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletIsAlreadyLoaded()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenNewPalletIsAssignedToOtherIssue()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenPalletsHaveDiffrentProducts()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenPalletsHaveDiffrentQuantityProducts()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenOldPalletIsPickingPalletAndHasMoreThanOneProduct()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
		[Fact]
		public void ReplacePalletInIssue_ShouldThrow_WhenNewPalletIsPickingPalletAndHasMoreThanOneProduct()
		{
			var issue = IssueTestData.CreateIssue(IssueStatus.Pending);
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
		}
	}
}
