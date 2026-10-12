using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueHistoryTests
	{
		[Fact]
		public void AddHistory_ShouldAdd()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var snapShot1 = location1.ToSnapshot();
			var location2 = IssueTestData.CreateLocation(2);
			var snapShot2 = location2.ToSnapshot();
			var issue = IssueTestData.CreateIssue(IssueStatus.ConfirmedToLoad);
			issue.AddIssueItem(product1.Id, 10, null, TestDates.TodayDateTime);
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
			issue.AddHistory(user);
			//Assert
			var historyIssue = Assert.Single(issue.DomainEvents.OfType<AddHistoryIssueNotification>());
			Assert.Empty(issue.HistoryIssues);
			Assert.Equal(user, historyIssue.UserId);
			Assert.Equal(1, historyIssue.ClientId);
			Assert.Equal(2, historyIssue.DetailDtos.Count);
			var details1 = historyIssue.DetailDtos.First(x => x.PalletId == pallet1.Id);
			Assert.Equal(location1.Id, details1.LocationId);
			Assert.Equal(snapShot1, details1.LocationSnapShot);
			var details2 = historyIssue.DetailDtos.First(x => x.PalletId == pallet2.Id);
			Assert.Equal(location2.Id, details2.LocationId);
			Assert.Equal(snapShot2, details2.LocationSnapShot);
			Assert.Equal(user, historyIssue.UserId);
			Assert.NotEmpty(historyIssue.Detailsitems);
			var details = historyIssue.Detailsitems.First();
			Assert.Equal(10, details.Quantity);
			Assert.Equal(product1.Id, details.ProductId);
		}
	}
}
