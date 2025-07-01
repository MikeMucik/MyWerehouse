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
	public class DeletePalletTests :CommandTestBase
	{
		private readonly PalletRepo _palletRepo;
		public DeletePalletTests(): base()
		{
			_palletRepo = new PalletRepo(_context);
		}
		//[Fact]
		//public void RemovePallet_DeletePalletById_RemoveFromList()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q1000",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.ToIssue,
		//		ReceiptId = 10,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q1011",
		//		DateReceived = DateTime.Now,
		//		LocationId = 2,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	_context.Pallets.AddRange(pallet1, pallet2);
		//	_context.SaveChanges();
		//	//Act
		//	var numberOfPallet = "Q1000";
		//	_palletRepo.DeletePallet(numberOfPallet);
		//	//Assert
		//	var result = _context.Pallets.Find(numberOfPallet);
		//	Assert.Null(result);
		//}
		//[Fact]
		//public async Task RemovePallet_DeletePalletByIdAsync_RemoveFromList()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q1000",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.ToIssue,
		//		ReceiptId = 10,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q1011",
		//		DateReceived = DateTime.Now,
		//		LocationId = 2,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	_context.Pallets.AddRange(pallet1, pallet2);
		//	_context.SaveChanges();
		//	//Act
		//	var numberOfPallet = "Q1000";
		//	await _palletRepo.DeletePalletAsync(numberOfPallet);
		//	//Assert
		//	var result = _context.Pallets.Find(numberOfPallet);
		//	Assert.Null(result);
		//}
	}
}
