using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.IntegrationTestRepo.HistoryTestsRepoSQLite
{
	[Collection("QueryCollectionInMemory")]
	public class ViewHistoryTests
	{
		private readonly HistoryPalletRepo _palletMovementRepo;
		public ViewHistoryTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_palletMovementRepo = new HistoryPalletRepo(_context);
		}		
		[Fact]
		public async Task GetHistoryPallet_ReturnHistory()
		{
			//Arrange
			var palletNumber = "Q1000";
			//Act
			var result =await _palletMovementRepo.GetHistoryPallet(palletNumber);
			// Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count);
			//  Sprawdzenie że wszystkie wyniki dotyczą tej samej palety
			Assert.All(result, r => Assert.Equal("Q1000", r.PalletNumber));
			//  Sprawdzenie że wszystkie mają Reason = Moved
			Assert.All(result, r => Assert.Equal(ReasonForPallet.Moved, r.Reason));
			//  Sprawdzenie że mają poprawną datę (>= MovementDateStart)
			Assert.All(result, r => Assert.True(r.MovementDate >= new DateTime(2025, 1, 1)));
			//  Sprawdzenie że użytkownik jest zgodny
			Assert.All(result, r => Assert.Equal("TestUser", r.PerformedBy));
			//  Sprawdzenie że każda pozycja ma detale produktów
			Assert.All(result, r => Assert.NotEmpty(r.HistoryPalletDetails));
			//  Dodatkowo można sprawdzić, że jedna z pozycji ma konkretny cel (np. DestinationLocationId = 2)
			Assert.Contains(result, r => r.DestinationLocationId == 2);
			Assert.Contains(result, r => r.DestinationLocationId == 3);


		}
		[Fact]
		public async Task CanDeletePalletAsync_ReturnFalse_IsCanDelete()
		{
			//Arrange		  
			var palletId =Guid.Parse("00000000-0001-1111-0000-000000000000");
			//Act
			var result =await _palletMovementRepo.CanDeletePalletAsync(palletId);
			//Assert
			Assert.False(result);
		}		
		[Fact]
		public async Task IsCanDelete_CanDeletePalletAsync_ReturnTrue()
		{
			//Arrange
			var palletId = Guid.Parse("00000000-0002-1111-0000-000000000000");
			//Act
			var result = await _palletMovementRepo.CanDeletePalletAsync(palletId);
			//Assert
			Assert.True(result);
		}
	}
}
