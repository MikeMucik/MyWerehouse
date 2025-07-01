using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ReceiptTestRepo
{
	public class AddReceiptTests: CommandTestBase
	{
		private readonly ReceiptRepo _receiptRepo;
		public AddReceiptTests(): base()
		{
			_receiptRepo = new ReceiptRepo(_context);
		}
		//[Fact]
		//public void AddReceipt_AddReceipt_AddToCollection()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q3000",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q3001",
		//		DateReceived = DateTime.Now,
		//		LocationId = 2,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	_context.Pallets.AddRange(pallet1, pallet2);
		//	_context.SaveChanges();		
		//	//Act	
		//	var receipt = new Receipt
		//	{				
		//		ReceiptDateTime = DateTime.Now,
		//		ClientId = 1,
		//		Pallets = new List<Pallet>
		//	{
		//		pallet1,
		//		pallet2
		//	},
		//		PerformedBy = "U005"
		//	};			
		//	_receiptRepo.AddReceipt(receipt);
		//	//Assert
		//	var result = _context.Receipts
		//		.Include(p => p.Pallets)
		//		.FirstOrDefault(i => i.Id == receipt.Id);
		//	Assert.NotNull(result);
		//	Assert.Equal(2, result.Pallets.Count);

		//	Assert.Equal("U005", result.PerformedBy);
		//	Assert.Equal(1, result.ClientId);
		//	Assert.Contains(result.Pallets, p => p.Id == "Q3000");
		//	Assert.Contains(result.Pallets, p => p.Id == "Q3001");

		//	foreach (var item in result.Pallets)
		//	{
		//		Assert.Equal(receipt.Id, item.ReceiptId);
		//	}
		//}
		//[Fact]
		//public async Task AddReceipt_AddReceiptAsync_AddToCollection()
		//{
		//	//Arrange
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q3000",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q3001",
		//		DateReceived = DateTime.Now,
		//		LocationId = 2,
		//		Status = PalletStatus.Available,
		//		ReceiptId = 10,
		//	};
		//	_context.Pallets.AddRange(pallet1, pallet2);
		//	_context.SaveChanges();
		//	//Act
		//	var receipt = new Receipt
		//	{
		//		ReceiptDateTime = DateTime.Now,
		//		ClientId = 1,
		//		Pallets = new List<Pallet>
		//	{
		//		pallet1,
		//		pallet2
		//	},
		//		PerformedBy = "U005"
		//	};			
		//	await _receiptRepo.AddReceiptAsync(receipt);
		//	//Assert
		//	var result = _context.Receipts
		//		.Include(p => p.Pallets)
		//		.FirstOrDefault(i => i.Id == receipt.Id);
		//	Assert.NotNull(result);
		//	Assert.Equal(2, result.Pallets.Count);

		//	Assert.Equal("U005", result.PerformedBy);
		//	Assert.Equal(1, result.ClientId);
		//	Assert.Contains(result.Pallets, p => p.Id == "Q3000");
		//	Assert.Contains(result.Pallets, p => p.Id == "Q3001");

		//	foreach (var item in result.Pallets)
		//	{
		//		Assert.Equal(receipt.Id, item.ReceiptId);
		//	}
		//}


	}
}
