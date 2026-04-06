using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
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
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletsAsync(productId2);			
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count); // powinny być dwie palety: Q1100 i Q1101 

			// Paleta Q1100
			var pallet1 = result.FirstOrDefault(vp => vp.Pallet.PalletNumber == "Q1100");
			Assert.NotNull(pallet1);
			Assert.Equal(200, pallet1.InitialPalletQuantity);
			Assert.Equal(3, pallet1.LocationId);
			Assert.Equal(3, pallet1.PickingTasks.Count);
			Assert.Equal(20, pallet1.PickingTasks.First().RequestedQuantity);
			Assert.Equal(PickingStatus.Allocated, pallet1.PickingTasks.First().PickingStatus);

			// Paleta Q1101
			var pallet2 = result.FirstOrDefault(vp => vp.Pallet.PalletNumber == "Q1101");
			Assert.NotNull(pallet2);
			Assert.Equal(150, pallet2.InitialPalletQuantity);
			Assert.Equal(3, pallet2.LocationId);
			Assert.Single(pallet2.PickingTasks);
			Assert.Equal(50, pallet2.PickingTasks.First().RequestedQuantity);
			Assert.Equal(PickingStatus.Allocated, pallet2.PickingTasks.First().PickingStatus);

			// Upewnij się, że nie zwrócono palety z innym produktem
			Assert.DoesNotContain(result, vp => vp.Pallet.PalletNumber == "Q1200");
		}
		[Fact]
		public async Task TakeVirtualPalletsByDates_GetVirtualPalletsAsync_ReturnList()
		{
			//Arrange
			var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");
			var palletGuid8 = Guid.Parse("00000000-0008-1111-0000-000000000000");

			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");

			//var startDate = new DateTime(2025, 5, 5);
			//var endDate = new DateTime(2025, 10, 10);
			var startDate = DateTime.UtcNow.AddDays(-2);
			var endDate = DateTime.UtcNow.AddDays(1);
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletsByTimeAsync(startDate, endDate);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count);
			// upewniamy się, że to właściwe palety
			var palletIds = result.Select(v => v.PalletId).ToList();
			//Assert.Contains("Q1100", palletIds);
			Assert.Contains(palletGuid5, palletIds);
			//Assert.Contains("Q1200", palletIds);
			Assert.Contains(palletGuid8, palletIds);

			// żadna inna spoza zakresu
			//Assert.DoesNotContain("Q1101", palletIds);
			Assert.DoesNotContain(palletGuid2, palletIds);

			// opcjonalnie: sprawdzamy daty, że rzeczywiście są w zakresie
			Assert.All(result, v =>
				Assert.InRange(v.DateMoved, startDate, endDate));
		}
		[Fact]
		public async Task TakeVirtualPalletsByPickingDates_GetVirtualPalletsAsync_ReturnList()
		{
			//Arrange
			//var startDate = new DateTime(2025, 5, 5);
			//var endDate = new DateTime(2025, 10, 10);
			var startDate =DateOnly.FromDateTime( DateTime.UtcNow.AddDays(-2));
			var endDate =DateOnly.FromDateTime( DateTime.UtcNow.AddDays(1));
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletsByTimePickingTaskAsync(startDate, endDate);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			//Assert.Equal(2, result.Count);
			//// upewniamy się, że to właściwe palety
			//var palletIds = result.Select(v => v.PalletId).ToList();
			//Assert.Contains("Q1100", palletIds);
			//Assert.Contains("Q1200", palletIds);

			//// żadna inna spoza zakresu
			//Assert.DoesNotContain("Q1101", palletIds);

			//// opcjonalnie: sprawdzamy daty, że rzeczywiście są w zakresie
			//Assert.All(result, v =>
			//	Assert.InRange(v.DateMoved, startDate, endDate));
		}
		//Task<List<VirtualPallet>> GetVirtualPalletsByTimePickingTaskAsync(DateOnly start, DateOnly end);


		[Fact]
		public async Task ReturnInt_GetVirtualPalletIdFromPalletIdAsync_ReturnVirtualPalletId()
		{
			//Arrange
			var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");
			var vpId1 = Guid.Parse("22222222-1111-2222-1111-111111111111");
			//var palletId = "Q1100";
			var palletId = palletGuid5;
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(palletId);
			//Assert
			Assert.NotEqual(Guid.Empty, result);
			Assert.Equal(vpId1, result);
		}
		[Fact]
		public async Task ReturnData_GetVirtualPalletByIdAsync_GiveBackProperData()
		{
			//Arrange
			var vpId1 = Guid.Parse("22222222-1111-2222-1111-111111111111");
			var virtualPalletId = 1;
			//Act
			var result = await _pickingPalletRepo.GetVirtualPalletByIdAsync(vpId1);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<VirtualPallet>(result);
			//Dodaj asercje

		}
	}
}
