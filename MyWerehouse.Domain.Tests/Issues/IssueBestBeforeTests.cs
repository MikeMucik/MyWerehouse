using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Warehouse.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
    public class IssueBestBeforeTests
    {
		[Fact]
		public void ReplacePalletInIssue_ShouldChange_WhenRequiredBestBeforeExistsAndNewPalletHasDate()
		{
			//Arrange
			var issueId = Guid.NewGuid();
			var productId = Guid.NewGuid();
			var location1 = new Location
			{
				Id = 1,
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var location2 = new Location
			{
				Id = 2,
				Bay = 1,
				Aisle = 1,
				Position = 2,
				Height = 1
			};
			
			var requiredBestBefore = new DateOnly(2027, 1, 10);
			var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
				DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "user", IssueStatus.Pending, null);
			var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, location1.Id,
				PalletStatus.LockedForIssue, Guid.NewGuid(), issueId);
			oldPallet.AddProduct(productId, 10, TestDates.UtcNow, requiredBestBefore);
			var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, location2.Id,
				PalletStatus.Available, Guid.NewGuid(), null);
			newPallet.AddProduct(productId, 10, TestDates.UtcNow, requiredBestBefore);
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
		}
		[Fact]
        public void ReplacePalletInIssue_ShouldThrow_WhenRequiredBestBeforeExistsAndNewPalletHasNoDate()
        {
            var issueId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var requiredBestBefore = new DateOnly(2027, 1, 10);
            var issue = Issue.CreateForSeed(issueId, 1, 1, TestDates.UtcNow,
                DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "user", IssueStatus.Pending, null);
            var oldPallet = Pallet.CreateForTests("OLD", TestDates.UtcNow, 1,
                PalletStatus.LockedForIssue, Guid.NewGuid(), issueId);
            oldPallet.AddProduct(productId, 10, TestDates.UtcNow, requiredBestBefore);
            var newPallet = Pallet.CreateForTests("NEW", TestDates.UtcNow, 2,
                PalletStatus.Available, Guid.NewGuid(), null);
            newPallet.AddProduct(productId, 10, TestDates.UtcNow, null);
            issue.Pallets.Add(oldPallet);
			//Act&Assert
			Assert.Throws<ProductOnPalletsAreNotTheSameBBDomainException>(() =>
                issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
        }
    }
}
