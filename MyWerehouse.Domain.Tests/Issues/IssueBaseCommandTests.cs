using MyWerehouse.Domain.Clients.ClientsExceptions;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Issuing.IssueExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Issues
{
	public class IssueBaseCommandTests
	{
		[Fact]
		public void CreateIssue_ShouldCreate_WhenProperData()
		{
			//Arrange
			var client = IssueTestData.CreateClient();
			var dateCreate = new DateTime(2025, 5, 5);
			var dateSending = DateOnly.FromDateTime(new DateTime(2025, 12, 15));
			var issueNumber = 1;
			var perfomedBy = "U003";
			//Act	
			var issue = Issue.Create(issueNumber, client.Id,
				dateSending, dateCreate, perfomedBy);
			//Assert
			Assert.NotNull(issue);
			Assert.Equal(dateCreate, issue.IssueDateTimeCreate);
			Assert.Equal(dateSending, issue.IssueDateTimeSend);
			Assert.Equal(issueNumber, issue.IssueNumber);
			Assert.Equal(perfomedBy, issue.PerformedBy);
			Assert.Equal(client.Id, issue.ClientId);
		}
		[Fact]
		public void CreateIssue_ShouldReturnValidationError_WhenNoClient()
		{
			//Arrange			
			var dateCreate = new DateTime(2025, 5, 5);
			var dateSending = DateOnly.FromDateTime(new DateTime(2025, 12, 15));
			var issueNumber = 1;
			var perfomedBy = "U003";
			var ex = Assert.Throws<ClientDomainException>(() => Issue.Create(issueNumber, 0,
				dateSending, dateCreate, perfomedBy));
			Assert.Equal("Client number must greater than zero. ", ex.Message);
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void CreateIssue_ShouldThrow_WhenSendingDateBeforeCreationDate()
		{
			//Arrange
			var client = IssueTestData.CreateClient();
			var dateCreate = new DateTime(2025, 5, 5, 12, 0, 0);
			var dateSending = new DateOnly(2025, 5, 4);
			var user = "U003";
			//Act&Assert
			Assert.Throws<WrongDateDomainException>(() =>
				Issue.Create(1, client.Id, dateSending, dateCreate, user));
		}
		[Fact]
		public void CreateIssue_ShouldReturnValidationError_WhenUserIsNull()
		{
			//Arrange
			var client = IssueTestData.CreateClient();
			var dateCreate = new DateTime(2025, 5, 5);
			var dateSending = new DateOnly(2025, 5, 6);
			//Act&Assert
			var ex = Assert.Throws<InvalidUserIdDomainException>(() =>
				Issue.Create(1, client.Id, dateSending, dateCreate, null!));
			Assert.Equal(ErrorType.Validation, ex.ErrorType);
		}
		[Fact]
		public void CreateIssue_ShouldCreate_WhenSendingDateIsCreationDay()
		{
			//Arrange
			var client = IssueTestData.CreateClient();
			var dateCreate = new DateTime(2025, 5, 5, 23, 59, 59);
			var dateSending = new DateOnly(2025, 5, 5);
			var user = "U003";
			//Act
			var issue = Issue.Create(1, client.Id, dateSending, dateCreate, user);
			//Assert
			Assert.Equal(dateCreate, issue.IssueDateTimeCreate);
			Assert.Equal(dateSending, issue.IssueDateTimeSend);
			Assert.Equal(IssueStatus.New, issue.IssueStatus);
			Assert.Equal(client.Id, issue.ClientId);
			Assert.Equal(user, issue.PerformedBy);
		}
		[Fact]
		public void AddIssueItem_ShouldAddItem_WhenQuantityAboveZero()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue();
			var product = Product.CreateForTests(Guid.NewGuid(),"NameofProduct", "ABC123", TestDates.DaysAgo(10), 1, false, 56);
			//Act
			issue.AddIssueItem(product.Id, 5, null, TestDates.TodayDateTime);
			//Assert
			Assert.Equal(5, issue.IssueItems.First(p=>p.ProductId == product.Id).Quantity);
		}
		[Fact]
		public void AddIssueItem_ShouldThrow_WhenProductAlreadyIsInIssue()
		{
			//Arrange
			var product = Product.CreateForTests(Guid.NewGuid(), "NameofProduct", "ABC123", TestDates.DaysAgo(10), 1, false, 56);
			var issue = IssueTestData.CreateIssue();
			var issueItem = IssueItem.CreateForSeed(1, issue.Id, product.Id, 5, null, TestDates.DaysAgo(1));

			issue.IssueItems.Add(issueItem);
			//Act&Assert
			var ex = Assert.Throws<ProductAlreadyExistDomainException>(() => issue.AddIssueItem(product.Id, 10, null, TestDates.TodayDateTime));
			
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			var item = Assert.Single(issue.IssueItems);
			Assert.Equal(5, item.Quantity);
		}
		[Fact]
		public void AddIssueItem_ReturnException_WhenQuantityBelowZero()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue();
			var product = Product.CreateForTests(Guid.NewGuid(), "NameofProduct", "ABC123", TestDates.DaysAgo(10), 1, false, 56);
			//Act&Assert
			var ex = Assert.Throws<InvalidQuantityIssueItemDomainException>(() => issue.AddIssueItem(product.Id, -5, null, TestDates.TodayDateTime));
			
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void AddIssueItem_ReturnException_WhenQuantityEqualZero()
		{
			//Arrange
			var issue = IssueTestData.CreateIssue();
			var product = Product.CreateForTests(Guid.NewGuid(), "NameofProduct", "ABC123", TestDates.DaysAgo(10), 1, false, 56);
			//Act&Assert
			var ex = Assert.Throws<InvalidQuantityIssueItemDomainException>(() => issue.AddIssueItem(product.Id, 0, null, TestDates.TodayDateTime));

			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		//RemoveNotLoadedPallets i DetachPallets
		[Fact]
		public void RemoveNotLoadedPallets_ReturnList()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue();
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
			var user = "user";
			//Act
			issue.RemoveNotLoadedPallets(user);
			//Assert
			Assert.DoesNotContain(pallet1, issue.Pallets);
			Assert.Equal(PalletStatus.Available, pallet1.Status);
		}
		[Fact]
		public void DetachPallets_ChangePalletStatus_WhenPalletHasReceiptId()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue();
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.ToIssue, Guid.NewGuid(), issue.Id);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.ToIssue, null, issue.Id);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			var user = "user";
			//Act
			issue.DetachPallets(user);
			//Assert
			Assert.Null(pallet1.IssueId);
			Assert.Equal(PalletStatus.Available, pallet1.Status);
			var historyPallet1Event = Assert.Single(pallet1.DomainEvents
				.OfType<PalletHistoryNotification>());

			Assert.Equal(pallet1.Id, historyPallet1Event.PalletId);
			Assert.Equal(PalletStatus.Available, historyPallet1Event.PalletStatus);
			Assert.Equal(user, historyPallet1Event.UserId);

		}

		[Fact]
		public void RemoveReceiptPalletsFromCollection_RemoveFromList()
		{
			//Arrange
			var product1 = IssueTestData.CreateProduct("Prod1", 1);
			var location1 = IssueTestData.CreateLocation(1);
			var location2 = IssueTestData.CreateLocation(2);
			var issue = IssueTestData.CreateIssue();
			var pallet1 = Pallet.CreateForTests("P1", TestDates.UtcNow, location1.Id, PalletStatus.ToIssue, Guid.NewGuid(), null);
			pallet1.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			var pallet2 = Pallet.CreateForTests("P2", TestDates.UtcNow, location2.Id, PalletStatus.ToIssue, null, null);
			pallet2.AddProduct(product1.Id, 10, TestDates.UtcNow, DateOnly.FromDateTime(TestDates.UtcNow.AddDays(366)));
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet1, location1);
			typeof(Pallet).GetProperty(nameof(Pallet.Location))!
				.SetValue(pallet2, location2);
			issue.Pallets.Add(pallet1);
			issue.Pallets.Add(pallet2);
			//Act
			issue.RemoveReceiptPalletsFromCollection();
			//Assert
			Assert.DoesNotContain(pallet1, issue.Pallets);
			Assert.Contains(pallet2, issue.Pallets);
		}
	}
}
