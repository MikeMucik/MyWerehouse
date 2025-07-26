using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
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
		public async Task NextId_GetNextPalletIdAsync_ReturnNextId()
		{
			//Arrange
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.ToIssue,
				ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			_context.Pallets.AddRange(pallet1, pallet2);
			_context.SaveChanges();
			//Act
			var result =await _palletRepo.GetNextPalletIdAsync();
			//Assert
			Assert.NotEmpty(result);
			Assert.Equal("Q1012", result);
		}

	}
}
