using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.IssueTests.Unit
{
    public class IssueBestBeforeTests
    {
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

            Assert.Throws<ProductOnPalletsAreNotTheSameBBDomainException>(() =>
                issue.ReplacePalletInIssue(oldPallet, newPallet, "user", requiredBestBefore));
        }
    }
}
