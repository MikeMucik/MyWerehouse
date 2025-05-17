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
	public class AddPalletTests :CommandTestBase		
	{
		private readonly PalletRepo _palletRepo;
		public AddPalletTests() :base() 
		{
			_palletRepo = new PalletRepo(_context);
		}
		[Fact]
		public void AddPallet_AddPallet_AddToCollection()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q00001",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			//Act
			var result = _palletRepo.AddPallet(pallet);
			//Assert
			Assert.NotNull(result);
			var createdPallet = _context.Pallets.Find(result);
			Assert.Equal(pallet.Status, createdPallet.Status);
			Assert.Equal(pallet.ReceiptId, createdPallet.ReceiptId);
		}
	}
}
