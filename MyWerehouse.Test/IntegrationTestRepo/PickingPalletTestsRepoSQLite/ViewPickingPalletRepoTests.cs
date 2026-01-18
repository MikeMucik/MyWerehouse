using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PickingPalletTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewPickingPalletRepoTests
	{
		private readonly PickingPalletRepo _pickingPalletRepo;
		private readonly QueryTestFixture _fixture;	
		public ViewPickingPalletRepoTests(QueryTestFixture fixture)
		{
			_fixture = fixture;
			_pickingPalletRepo = new PickingPalletRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task TakeVirtualPallets_GetVirtualPalletsAsync_ReturnList()
		{
			//Arrange
			var productId = 11;
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletsAsync(productId);			
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count); // powinny być dwie palety: Q1100 i Q1101 

			// Paleta Q1100
			var pallet1 = result.FirstOrDefault(vp => vp.PalletId == "Q1100");
			Assert.NotNull(pallet1);
			Assert.Equal(200, pallet1.InitialPalletQuantity);
			Assert.Equal(3, pallet1.LocationId);
			Assert.Equal(2, pallet1.Allocations.Count);
			Assert.Equal(20, pallet1.Allocations.First().Quantity);
			Assert.Equal(PickingStatus.Allocated, pallet1.Allocations.First().PickingStatus);

			// Paleta Q1101
			var pallet2 = result.FirstOrDefault(vp => vp.PalletId == "Q1101");
			Assert.NotNull(pallet2);
			Assert.Equal(150, pallet2.InitialPalletQuantity);
			Assert.Equal(3, pallet2.LocationId);
			Assert.Single(pallet2.Allocations);
			Assert.Equal(50, pallet2.Allocations.First().Quantity);
			Assert.Equal(PickingStatus.Allocated, pallet2.Allocations.First().PickingStatus);

			// Upewnij się, że nie zwrócono palety z innym produktem
			Assert.DoesNotContain(result, vp => vp.PalletId == "Q1200");
		}
		[Fact]
		public async Task TakeVirtualPalletsByDates_GetVirtualPalletsAsync_ReturnList()
		{
			//Arrange
			var startDate = new DateTime(2025, 5, 5);
			var endDate = new DateTime(2025, 10, 10);
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(startDate, endDate);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count);
			// upewniamy się, że to właściwe palety
			var palletIds = result.Select(v => v.PalletId).ToList();
			Assert.Contains("Q1100", palletIds);
			Assert.Contains("Q1200", palletIds);

			// żadna inna spoza zakresu
			Assert.DoesNotContain("Q1101", palletIds);

			// opcjonalnie: sprawdzamy daty, że rzeczywiście są w zakresie
			Assert.All(result, v =>
				Assert.InRange(v.DateMoved, startDate, endDate));
		}
		[Fact]
		public async Task ReturnInt_GetVirtualPalletIdFromPalletIdAsync_ReturnVirtualPalletId()
		{
			//Arrange
			var palletId = "Q1100";
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletId);
			//Assert
			Assert.NotEqual(0, result);
			Assert.Equal(1, result);
		}
		[Fact]
		public async Task ReturnData_GetVirtualPalletByIdAsync_GiveBackProperData()
		{
			//Arrange
			var virtualPalletId = 1;
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletByIdAsync(virtualPalletId);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<VirtualPallet>(result);
			//Dodaj asercje

		}
	}
}
