using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.UnitTestRepo.ReceiptTestRepo
{
	public class UpdateReceiptTests
	{
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public UpdateReceiptTests()
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("TestDatabase")
				.Options;
		}
		//[Fact]
		//public void UpdateReceiptData_UpdateReceipt_ChangeData()
		//{
		//	//Arrange
		//	var updatingReceipt = new Receipt
		//	{
		//		Id = 10,
		//		ClientId = 1,
		//		ReceiptDateTime = new DateTime(2025, 4, 4),
		//		PerformedBy = "U0099",
		//		Pallets = new List<Pallet>{
		//			new Pallet { Id = "999" }, new Pallet {Id = "998"}
		//		}
		//	};
		//	using var arrangeContext = new WerehouseDbContext(_contextOptions);
		//	arrangeContext.Receipts.Add(updatingReceipt);
		//	arrangeContext.SaveChanges();
		//	//Act
		//	var updatedReceipt = new Receipt
		//	{
		//		Id = 10,
		//		ClientId = 1,
		//		ReceiptDateTime = new DateTime(2026, 3, 3),
		//		PerformedBy = "U0098",
		//		Pallets = new List<Pallet>{
		//			new Pallet { Id = "999" },
		//			new Pallet {Id = "998"},
		//			new Pallet{Id ="997"}
		//		}
		//	};
		//	using (var actContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var repo = new ReceiptRepo(actContext);
		//		repo.UpdateReceipt(updatedReceipt);
		//	}
		//	//Assert
		//	using (var assertContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var result = assertContext.Receipts.FirstOrDefault(x => x.Id == updatingReceipt.Id);

		//		Assert.NotNull(result);
		//		//Assert.Equal(updatedReceipt.PerformedBy, result.PerformedBy);
		//		Assert.Equal(updatedReceipt.ReceiptDateTime, result.ReceiptDateTime);
		//	}
		//}
		//[Fact]
		//public async Task UpdateReceiptData_UpdateReceiptAsync_ChangeData()
		//{
		//	//Arrange
		//	var updatingReceipt = new Receipt
		//	{
		//		Id = 11,
		//		ClientId = 1,
		//		ReceiptDateTime = new DateTime(2025, 4, 4),
		//		PerformedBy = "U0099",
		//		Pallets = new List<Pallet>{
		//			new Pallet { Id = "0999" }, new Pallet {Id = "0998"}
		//		}
		//	};
		//	using var arrangeContext = new WerehouseDbContext(_contextOptions);
		//	arrangeContext.Receipts.Add(updatingReceipt);
		//	arrangeContext.SaveChanges();
		//	//Act
		//	var updatedReceipt = new Receipt
		//	{
		//		Id = 11,
		//		ClientId = 1,
		//		ReceiptDateTime = new DateTime(2026, 3, 3),
		//		PerformedBy = "U0098",
		//		Pallets = new List<Pallet>{
		//			new Pallet { Id = "0999" },
		//			new Pallet {Id = "0998"},
		//			new Pallet{Id ="0997"}
		//		}
		//	};
		//	using (var actContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var repo = new ReceiptRepo(actContext);
		//		await repo.UpdateReceiptAsync(updatedReceipt);
		//	}
		//	//Assert
		//	using (var assertContext = new WerehouseDbContext(_contextOptions))
		//	{
		//		var result = assertContext.Receipts.FirstOrDefault(x => x.Id == updatingReceipt.Id);

		//		Assert.NotNull(result);
		//		//Assert.Equal(updatedReceipt.PerformedBy, result.PerformedBy);
		//		Assert.Equal(updatedReceipt.ReceiptDateTime, result.ReceiptDateTime);
		//	}
		//}
	}
}
