using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Filters;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestRepo.HistoryTestsRepoSQLite
{
	[Collection("QuerryCollection")]
	public class ViewHistoryTests : CommandTestBase
	{
		private readonly PalletMovementRepo _palletMovementRepo;
		public ViewHistoryTests(QuerryTestFixture fixture)
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
				ProductName = "Test",
				MovementDateStart = new DateTime(2025,1,1)				
			};
			//Act
			var result = _palletMovementRepo.GetDataByFilter(filter, "Q1000");
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			//  Sprawdzenie że wszystkie wyniki dotyczą tej samej palety
			Assert.All(result, r => Assert.Equal("Q1000", r.PalletId));

			//  Sprawdzenie że wszystkie mają Reason = Moved
			Assert.All(result, r => Assert.Equal(ReasonMovement.Moved, r.Reason));

			//  Sprawdzenie że mają poprawną datę (>= MovementDateStart)
			Assert.All(result, r => Assert.True(r.MovementDate >= new DateTime(2025, 1, 1)));

			//  Sprawdzenie że użytkownik jest zgodny
			Assert.All(result, r => Assert.Equal("TestUser", r.PerformedBy));

			//  Sprawdzenie że każda pozycja ma detale produktów
			Assert.All(result, r => Assert.NotEmpty(r.PalletMovementDetails));

			//  Sprawdzenie że w detalach znajdują się poprawne produkty
			Assert.All(result, r =>
				Assert.Contains(r.PalletMovementDetails, d => d.ProductId == 10 && d.Quantity > 0));

			//  Dodatkowo można sprawdzić, że jedna z pozycji ma konkretny cel (np. DestinationLocationId = 2)
			Assert.Contains(result, r => r.DestinationLocationId == 2);
			Assert.Contains(result, r => r.DestinationLocationId == 3);
		}
		[Fact]
		public void ShowRecordByFilter_GetDataByFilter_ShowNull()
		{
			//Arrange
			var filter = new PalletMovementSearchFilter
			{							
				MovementDateStart = new DateTime(2025, 3, 3)
			};
			//Act
			var result = _palletMovementRepo.GetDataByFilter(filter, "Q1001");
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
		//[Fact]
		//public async Task ShowRecordByFilter_
	}
}
