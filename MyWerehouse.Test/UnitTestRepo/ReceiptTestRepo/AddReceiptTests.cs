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
		[Fact]
		public void AddReceipt_AddReceipt_AddToCollection()
		{			
			//Arrange
			var pallet1 = new Pallet
			{
				Id = "Q3000"
			};
			var pallet2 = new Pallet
			{
				Id = "Q3001"
			};
			var pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			};
			var receipt = new Receipt
			{				
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = pallets,
				PerformedBy = "U005"
			};
			//Act
			_receiptRepo.AddReceipt(receipt);
			//Assert
			var result = _context.Receipts
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U005", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q3000");
			Assert.Contains(result.Pallets, p => p.Id == "Q3001");

			foreach (var item in result.Pallets)
			{
				Assert.Equal(receipt.Id, item.ReceiptId);
			}
		}
		[Fact]
		public async Task AddReceipt_AddReceiptAsync_AddToCollection()
		{
			//Arrange
			var pallet1 = new Pallet
			{
				Id = "Q3000"
			};
			var pallet2 = new Pallet
			{
				Id = "Q3001"
			};
			var pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			};
			var receipt = new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = pallets,
				PerformedBy = "U005"
			};
			//Act
			await _receiptRepo.AddReceiptAsync(receipt);
			//Assert
			var result = _context.Receipts
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U005", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q3000");
			Assert.Contains(result.Pallets, p => p.Id == "Q3001");

			foreach (var item in result.Pallets)
			{
				Assert.Equal(receipt.Id, item.ReceiptId);
			}
		}
		[Fact]
		public void AddIssueExistingPallets_AddReceipt_AddToCollection()
		{
			//Arrange
			var pallet1 = _context.Pallets.Find("Q1000");
			var pallet2 = _context.Pallets.Find("Q1001");
			var pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			};

			var receipt = new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = pallets,
				PerformedBy = "U003"
			};
			//Act
			_receiptRepo.AddReceipt(receipt);
			//Assert
			var result = _context.Receipts
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U003", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q1000");
			Assert.Contains(result.Pallets, p => p.Id == "Q1001");

			foreach (var item in result.Pallets)
			{
				Assert.Equal(receipt.Id, item.ReceiptId);
			}
		}
		[Fact]
		public async Task AddIssueExistingPallets_AddReceiptAsync_AddToCollection()
		{
			//Arrange
			var pallet1 = _context.Pallets.Find("Q1000");
			var pallet2 = _context.Pallets.Find("Q1001");
			var pallets = new List<Pallet>
			{
				pallet1,
				pallet2
			};

			var receipt = new Receipt
			{
				ReceiptDateTime = DateTime.Now,
				ClientId = 1,
				Pallets = pallets,
				PerformedBy = "U003"
			};
			//Act
			await _receiptRepo.AddReceiptAsync(receipt);
			//Assert
			var result = _context.Receipts
				.Include(p => p.Pallets)
				.FirstOrDefault(i => i.Id == receipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);

			Assert.Equal("U003", result.PerformedBy);
			Assert.Equal(1, result.ClientId);
			Assert.Contains(result.Pallets, p => p.Id == "Q1000");
			Assert.Contains(result.Pallets, p => p.Id == "Q1001");

			foreach (var item in result.Pallets)
			{
				Assert.Equal(receipt.Id, item.ReceiptId);
			}
		}		
	}
}
