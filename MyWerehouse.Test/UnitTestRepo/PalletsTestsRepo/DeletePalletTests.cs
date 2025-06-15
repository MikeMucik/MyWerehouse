using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.PalletsTestsRepo
{
	public class DeletePalletTests :CommandTestBase
	{
		private readonly PalletRepo _palletRepo;
		public DeletePalletTests(): base()
		{
			_palletRepo = new PalletRepo(_context);
		}
		[Fact]
		public void RemovePallet_DeletePalletById_RemoveFromList()
		{
			//Arrange
			var numberOfPallet = "Q1000"; 
			//Act
			_palletRepo.DeletePallet(numberOfPallet);
			//Assert
			var result = _context.Pallets.Find(numberOfPallet);
			Assert.Null(result);
		}
		[Fact]
		public async Task RemovePallet_DeletePalletByIdAsync_RemoveFromList()
		{
			//Arrange
			var numberOfPallet = "Q1000";
			//Act
			await _palletRepo.DeletePalletAsync(numberOfPallet);
			//Assert
			var result = _context.Pallets.Find(numberOfPallet);
			Assert.Null(result);
		}
	}
}
