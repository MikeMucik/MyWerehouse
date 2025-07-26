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
	[Collection("QuerryCollection")]
	public class ViewPalletMovementTests : CommandTestBase
	{
		private readonly PalletMovementRepo _palletMovementRepo;
		public ViewPalletMovementTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_palletMovementRepo = new PalletMovementRepo(_context);
		}
		[Fact]
		public void ShowRecordByFilter_GetDataByFilter_ShowList()
		{
			//Arrange
			var filter = new PalletMovementSearchFilter
			{
				PalletId = "Q1000",
				ProductName = "Test",
				MovementDateStart = new DateTime(2025,1,1)				
			};
			//Act
			var result = _palletMovementRepo.GetDataByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count()); 
		}
		[Fact]
		public void ShowRecordByFilter_GetDataByFilter_ShowNull()
		{
			//Arrange
			var filter = new PalletMovementSearchFilter
			{
				PalletId = "Q1001",				
				MovementDateStart = new DateTime(2025, 3, 3)
			};
			//Act
			var result = _palletMovementRepo.GetDataByFilter(filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(0, result.Count());
		}		
		[Fact]
		public async Task IsCanDelete_CanDeletePalletAsync_ReturnFalse()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			var result =await _palletMovementRepo.CanDeletePalletAsync(palletId);
			//Assert
			Assert.False(result);
		}		
		[Fact]
		public async Task IsCanDelete_CanDeletePalletAsync_ReturnTrue()
		{
			//Arrange
			var palletId = "Q1001";
			//Act
			var result = await _palletMovementRepo.CanDeletePalletAsync(palletId);
			//Assert
			Assert.True(result);
		}
	}
}
