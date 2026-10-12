using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Events;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Test;
using TestSupport;
using TestDates = TestSupport.TestDates;

namespace MyWerehouse.Domain.Tests.Pallets
{
	public class PalletPickingTests
	{
		//CreatePickingPallet, AssignToPicking, EnsureCanBeUsedForPicking, PickProduct, CloseAndAddPickingPallet
		[Fact]
		public void CreatePickingPallet_ShouldCreate_WhenOk()
		{
			//Arrange
			var palletNumber = "Q1000";
			var dateCreated = TestDates.Now;
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			var product = PalletTestData.CreateProduct("eggs", 1);
			var location = PalletTestData.CreateLocation(1);
			var quantity = 100;
			//Act
			var pallet = Pallet.CreatePickingPallet(palletNumber, location.Id, dateCreated, product.Id, quantity, dateBB);
			//Assert
			Assert.Equal(PalletStatus.Picking, pallet.Status);
			Assert.Equal(quantity, pallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(location.Id, pallet.LocationId);
			Assert.Equal(dateCreated, pallet.ProductsOnPallet.First().DateAdded);
			Assert.Equal(dateBB, pallet.ProductsOnPallet.First().BestBefore);
		}
		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		public void AssignToPicking_ShouldChangeStatusSendDomainEvent_WhenStatusAllowed(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var user = "User";
			var snapShot = location.ToSnapshot();
			//Act
			pallet.AssignToPicking(user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.ToPicking, pallet.Status);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(Histories.Models.ReasonForPallet.Picking, historyPallet.ReasonMovement);
		}
		[Theory]
		[InlineData(PalletStatus.ToPicking)]
		[InlineData(PalletStatus.ToIssue)]
		[InlineData(PalletStatus.Damaged)]
		[InlineData(PalletStatus.OnHold)]
		[InlineData(PalletStatus.Receiving)]
		[InlineData(PalletStatus.LockedForIssue)]
		[InlineData(PalletStatus.Picking)]
		[InlineData(PalletStatus.ReversePicking)]
		[InlineData(PalletStatus.New)]
		[InlineData(PalletStatus.Loaded)]
		[InlineData(PalletStatus.Archived)]
		[InlineData(PalletStatus.Cancelled)]
		//[InlineData(PalletStatus.Picking)]
		public void AssignToPicking_ThrowError_WhenStatusNotAllowed(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var location = PalletTestData.CreateLocation(1);
			var user = "User";
			var snapShot = location.ToSnapshot();
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.AssignToPicking(user, snapShot));
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
			Assert.Equal(status, pallet.Status);
			Assert.Empty(pallet.DomainEvents);
		}
		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.ToPicking)]
		public void EnsureCanBeUsedForPicking_ShouldReturnProductOnPallet_WhenCorrectStatusAndProduct(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			//Act
			var result = pallet.EnsureCanBeUsedForPicking();
			//Assert
			Assert.NotNull(result);
			Assert.IsType<ProductOnPallet>(result);
			Assert.True(pallet.ProductsOnPallet.Single().Quantity > 0);
		}

		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.ToPicking)]
		public void EnsureCanBeUsedForPicking_ReturnError_WhenTwoProductsOnPallet(PalletStatus status)
		{
			//Arrange
			var product = PalletTestData.CreateProduct("knives", 1);
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			pallet.AddProduct(product.Id, 10, TestDates.DaysAgo(1), null);
			//Act&Assert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(() =>
			pallet.EnsureCanBeUsedForPicking());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}

		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.ToPicking)]
		public void EnsureCanBeUsedForPicking_ReturnError_WhenPalletIsEmpty(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletWithStatus(status); //No products
			//Act&Assert
			var ex = Assert.Throws<PalletMustContainSingleProductLineDomainException>(() =>
			pallet.EnsureCanBeUsedForPicking());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}

		[Theory]
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.ToPicking)]
		public void EnsureCanBeUsedForPicking_ReturnError_WhenInQuantityZero(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			var product = pallet.ProductsOnPallet.FirstOrDefault();
			product!.SetQuantity(0);
			//Act&Assert
			var ex = Assert.Throws<InsufficientQuantityDomainException>(() =>
			pallet.EnsureCanBeUsedForPicking());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		
		[Theory]
		[InlineData(PalletStatus.ToIssue)]//1
		[InlineData(PalletStatus.Damaged)]//2
		[InlineData(PalletStatus.OnHold)]//3
		[InlineData(PalletStatus.Loaded)]//4
		[InlineData(PalletStatus.Receiving)]//5
		[InlineData(PalletStatus.LockedForIssue)]//6
		[InlineData(PalletStatus.Picking)]//7
		[InlineData(PalletStatus.ReversePicking)]//8
		[InlineData(PalletStatus.New)]//9
		[InlineData(PalletStatus.Cancelled)]//10
		[InlineData(PalletStatus.Archived)]//11
		public void EnsureCanBeUsedForPicking_ReturnError_WhenInCorrectStatusAndProduct(PalletStatus status)
		{
			//Arrange
			var pallet = PalletTestData.CreatePalletFromReceipt("Q1000", 1, status);
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			 pallet.EnsureCanBeUsedForPicking());
			Assert.Equal(ErrorType.Conflict, ex.ErrorType);
		}
		[Fact]
		public void PickProduct_ReduceQuantitySendDomainEvent_WhenAllowedQuantity()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("potato", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var pallet = PalletTestData.CreatePallet();
			pallet.ChangeStatus(PalletStatus.ToPicking);
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(10), TestDates.DateBestBeforeInMonths(10));
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			var productOnPallet = pallet.GetProductOnPallet(product.Id, dateBB);
			var user = "User";
			//Act
			pallet.PickProduct(productOnPallet, 10, user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.ToPicking, pallet.Status);
			Assert.Equal(90, pallet.ProductsOnPallet.Single().Quantity);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Picking, historyPallet.ReasonMovement);
		}
		[Fact]
		public void PickProduct_ReduceQuantitySendDomainEventToArchive_WhenQuantityReducedToZero()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("potato", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var pallet = PalletTestData.CreatePallet();
			pallet.ChangeStatus(PalletStatus.ToPicking);
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(10), TestDates.DateBestBeforeInMonths(10));
			var dateBB = TestDates.DateBestBeforeInMonths(1);
			var productOnPallet = pallet.GetProductOnPallet(product.Id, dateBB);
			var user = "User";
			//Act
			pallet.PickProduct(productOnPallet, 100, user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.Archived, pallet.Status);
			Assert.Equal(0, pallet.ProductsOnPallet.Single().Quantity);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.Picking, historyPallet.ReasonMovement);
		}
		[Fact]
		public void CloseAndAddPickingPallet_ChangeStatusToIssue_WhenStatusPicking()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("potato", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var pallet = PalletTestData.CreatePallet();
			pallet.ChangeStatus(PalletStatus.Picking);
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(10), TestDates.DateBestBeforeInMonths(10));
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act
			pallet.CloseAndAddPickingPallet(issueId, user, snapShot);
			//Assert
			Assert.Equal(PalletStatus.ToIssue, pallet.Status);
			Assert.Equal(issueId, pallet.IssueId);
			var historyPallet = Assert.Single(pallet.DomainEvents.OfType<PalletHistoryNotification>());
			Assert.Equal(ReasonForPallet.ToLoad, historyPallet.ReasonMovement);
		}
		[Fact]
		public void CloseAndAddPickingPallet_ThrowException_WhenStatusToIssue()
		{
			//Arrange
			var product = PalletTestData.CreateProduct("potato", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var pallet = PalletTestData.CreatePallet();
			pallet.ChangeStatus(PalletStatus.ToIssue);
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(10), TestDates.DateBestBeforeInMonths(10));
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<AlreadyAssignedDomainException>(()=>
			pallet.CloseAndAddPickingPallet(issueId, user, snapShot));
			
			Assert.Equal(PalletStatus.ToIssue, pallet.Status);
			Assert.Null(pallet.IssueId);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}
		[Theory]
		[InlineData(PalletStatus.Damaged)]//1
		[InlineData(PalletStatus.OnHold)]//2
		[InlineData(PalletStatus.Loaded)]//3
		[InlineData(PalletStatus.Receiving)]//4
		[InlineData(PalletStatus.LockedForIssue)]//5		
		[InlineData(PalletStatus.ReversePicking)]//6
		[InlineData(PalletStatus.New)]//7
		[InlineData(PalletStatus.Cancelled)]//8
		[InlineData(PalletStatus.Archived)]//9
		[InlineData(PalletStatus.Available)]
		[InlineData(PalletStatus.InStock)]
		[InlineData(PalletStatus.ToPicking)]
		public void CloseAndAddPickingPallet_ThrowException_WhenWrongStatus(PalletStatus status)
		{
			//Arrange
			var product = PalletTestData.CreateProduct("potato", 1);
			var location = PalletTestData.CreateLocation(1);
			var snapShot = location.ToSnapshot();
			var pallet = PalletTestData.CreatePallet();
			pallet.ChangeStatus(status);
			pallet.AddProduct(product.Id, 100, TestDates.DaysAgo(10), TestDates.DateBestBeforeInMonths(10));
			var issueId = Guid.NewGuid();
			var user = "User";
			//Act&Assert
			var ex = Assert.Throws<InvalidPalletStatusDomainException>(() =>
			pallet.CloseAndAddPickingPallet(issueId, user, snapShot));

			Assert.Equal(status, pallet.Status);
			Assert.Null(pallet.IssueId);
			Assert.Empty(pallet.DomainEvents.OfType<PalletHistoryNotification>());
		}
	}
}
