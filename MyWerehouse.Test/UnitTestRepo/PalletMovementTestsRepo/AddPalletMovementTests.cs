using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.PalletMovementTestsRepo
{
	public class AddPalletMovementTests : CommandTestBase
	{
		private readonly PalletMovementRepo _palletMovementRepo;
		public AddPalletMovementTests():base()
		{
			_palletMovementRepo = new PalletMovementRepo(_context);
		}
		//[Fact]
		//public void AddRecord_AddPalletMovement_AddToList()
		//{
		//	//Arrange
		//	var pallletMovement = new PalletMovement
		//	{
		//		PalletId = "Q1000",
		//		ProductId = 10,
		//		LocationId = 10,
		//		Reason = ReasonMovement.ManualMove,
		//		PerformedBy = "U001",
		//		Quantity = -1,
		//		MovementDate = DateTime.Now,
		//	};
		//	//Act
		//	_palletMovementRepo.AddPalletMovement(pallletMovement);
		//	//Assert			
		//	var resultList = _context.PalletMovement.Where(m => m.PalletId == "Q1000");
		//	var result = resultList				
		//		.OrderByDescending(p=>p.MovementDate)
		//		.FirstOrDefault();
		//	Assert.NotNull(result);
		//	Assert.Equal(3, resultList.Count()); // 2 record from DbContextFactory
		//	Assert.Equal(-1, resultList.First(p=>p.PerformedBy == "U001").Quantity);
		//	Assert.Equal(1, resultList.First(p=>p.MovementDate == new DateTime(2025,2,2)).Quantity);

		//	Assert.Equal("U001", result.PerformedBy);
		//	Assert.Equal(-1, result.Quantity);
		//	Assert.Equal(ReasonMovement.ManualMove, result.Reason);

		//	var historicalRecord = resultList.FirstOrDefault(p => p.MovementDate == new DateTime(2025, 2, 2));
		//	Assert.NotNull(historicalRecord);
		//	Assert.Equal(1, historicalRecord.Quantity);
		//}
		//[Fact]
		//public async Task AddRecord_AddPalletMovementAsync_AddToList()
		//{
		//	//Arrange
		//	var pallletMovement = new PalletMovement
		//	{
		//		PalletId = "Q1000",
		//		ProductId = 10,
		//		LocationId = 10,
		//		Reason = ReasonMovement.ManualMove,
		//		PerformedBy = "U001",
		//		Quantity = -1,
		//		MovementDate = DateTime.Now,
		//	};
		//	//Act
		//	await _palletMovementRepo.AddPalletMovementAsync(pallletMovement);
		//	//Assert			
		//	var resultList = _context.PalletMovement.Where(m => m.PalletId == "Q1000");
		//	var result = resultList
		//		.OrderByDescending(p => p.MovementDate)
		//		.FirstOrDefault();
		//	Assert.NotNull(result);
		//	Assert.Equal(3, resultList.Count()); // 2 record from DbContextFactory
		//	Assert.Equal(-1, resultList.First(p => p.PerformedBy == "U001").Quantity);
		//	Assert.Equal(1, resultList.First(p => p.MovementDate == new DateTime(2025, 2, 2)).Quantity);

		//	Assert.Equal("U001", result.PerformedBy);
		//	Assert.Equal(-1, result.Quantity);
		//	Assert.Equal(ReasonMovement.ManualMove, result.Reason);

		//	var historicalRecord = resultList.FirstOrDefault(p => p.MovementDate == new DateTime(2025, 2, 2));
		//	Assert.NotNull(historicalRecord);
		//	Assert.Equal(1, historicalRecord.Quantity);
		//}
	}
}
