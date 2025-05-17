using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ReceiptTestRepo
{
	public class DeleteReceiptTests :CommandTestBase
	{
		private readonly ReceiptRepo _receiptRepo;
		public DeleteReceiptTests(): base()
		{
			_receiptRepo= new ReceiptRepo(_context);
		}
		[Fact]
		public void RemoveReceipt_DeleteReceipt_RemoveRecordFromList()
		{
			//Arrange
			var id = 1;
			//Act
			_receiptRepo.DeleteReceipt(id);
			//Assert
			var receipt = _context.Issues.Find(id);
			Assert.Null(receipt);
		}
	}
}
