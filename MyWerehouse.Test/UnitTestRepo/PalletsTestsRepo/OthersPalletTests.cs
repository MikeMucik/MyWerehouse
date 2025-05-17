using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.PalletsTestsRepo
{
	public class OthersPalletTests:CommandTestBase
	{
		private readonly PalletRepo _palletRepo;
		public OthersPalletTests() : base()
		{
			_palletRepo = new PalletRepo(_context);
		}
		[Fact]
		public void RemoveFromReceipt_ClearPalletFromListReceipt_ChangeProperyties()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			_palletRepo.ClearPalletFromListReceipt(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p=>p.Id == palletId);
			Assert.NotNull(result);
			Assert.Equal(0, result.ReceiptId);
			Assert.Equal(Domain.Models.PalletStatus.Available, result.Status);
		}
		[Fact]
		public void RemoveFromIssue_ClearPalletFromListReceipt_ChangeProperyties()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			_palletRepo.ClearPalletFromListIssue(palletId);
			//Assert
			var result = _context.Pallets.FirstOrDefault(p => p.Id == palletId);
			Assert.NotNull(result);
			Assert.Null(result.IssueId);
			Assert.Equal(Domain.Models.PalletStatus.Available, result.Status);
		}

	}
}
