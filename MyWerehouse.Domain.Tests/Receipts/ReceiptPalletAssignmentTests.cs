using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using TestSupport;

namespace MyWerehouse.Domain.Tests.Receipts
{
	public class ReceiptPalletAssignmentTests
	{
		//DetachPallet
		
		[Fact]
		public void AttachPallet_ShouldAdd()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet();
			var pallet = Pallet.Create("Q1000", 1, TestDates.DaysAgo(1));
			//Act
			receipt.AttachPallet(pallet);
			//Assert
			Assert.Contains(pallet, receipt.Pallets);
			Assert.Empty(receipt.DomainEvents);
		}
		[Fact]
		public void AttachPallet_ShouldAdd_WhenAgainAttach()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet();
			var pallet = Pallet.Create("Q1000", 1, TestDates.DaysAgo(1));
			receipt.AttachPallet(pallet);
			//Act
			receipt.AttachPallet(pallet);
			//Assert
			var result = receipt.Pallets.Single(x => x.Id == pallet.Id);
			Assert.Null(result.ReceiptId);
			Assert.Contains(pallet, receipt.Pallets);
			Assert.Empty(receipt.DomainEvents);
		}
		[Fact]
		public void AttachPallet_ShouldAdd_WhenSecondAdded()
		{
			//Arrange
			var receipt = ReceiptTestData.CreateReceiptWithPallet();
			var pallet = Pallet.Create("Q1000", 1, TestDates.DaysAgo(1));
			var pallet1 = Pallet.Create("Q1001", 1, TestDates.DaysAgo(1));
			receipt.AttachPallet(pallet);
			//Act
			receipt.AttachPallet(pallet1);
			//Assert
			var result = receipt.Pallets.Single(x => x.Id == pallet.Id);
			var result1 = receipt.Pallets.Single(x => x.Id == pallet1.Id);
			Assert.Null(result.ReceiptId);
			Assert.Contains(pallet, receipt.Pallets);
			Assert.Contains(pallet1, receipt.Pallets);
			Assert.Empty(receipt.DomainEvents);
		}
		//DetachPallet 
	}
}
