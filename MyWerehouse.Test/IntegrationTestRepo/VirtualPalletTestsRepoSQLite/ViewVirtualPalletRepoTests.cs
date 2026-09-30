using MyWerehouse.Domain.Pickings.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.VirtualPalletTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewVirtualPalletRepoTests
	{
		private readonly VirtualPalletRepo _virtualPalletRepo;
		private readonly QueryTestSQLFixture _fixture;
		public ViewVirtualPalletRepoTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_virtualPalletRepo = new VirtualPalletRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task TakeVirtualPallets_GetVirtualPalletsAsync_ReturnList()
		{
			//Arrange
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");

			//Act
			var result = await _virtualPalletRepo.GetVirtualPalletsAsync(productId2, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count); // There should be two pallets: Q1100 and Q1101

			// Pallet Q1100
			var pallet1 = result.FirstOrDefault(vp => vp.Pallet.PalletNumber == "Q1100");
			Assert.NotNull(pallet1);
			Assert.Equal(200, pallet1.InitialPalletQuantity);
			Assert.Equal(3, pallet1.LocationId);
			Assert.Equal(3, pallet1.PickingTasks.Count);
			Assert.Equal(20, pallet1.PickingTasks.First().RequestedQuantity);
			Assert.Equal(PickingStatus.Allocated, pallet1.PickingTasks.First().PickingStatus);

			// Pallet Q1101
			var pallet2 = result.FirstOrDefault(vp => vp.Pallet.PalletNumber == "Q1101");
			Assert.NotNull(pallet2);
			Assert.Equal(150, pallet2.InitialPalletQuantity);
			Assert.Equal(3, pallet2.LocationId);
			Assert.Single(pallet2.PickingTasks);
			Assert.Equal(50, pallet2.PickingTasks.First().RequestedQuantity);
			Assert.Equal(PickingStatus.Allocated, pallet2.PickingTasks.First().PickingStatus);

			// Verify that no pallet with a different product was returned
			Assert.DoesNotContain(result, vp => vp.Pallet.PalletNumber == "Q1200");
		}
		[Fact]
		public async Task TakeVirtualPalletsByDates_GetVirtualPalletsAsync_ReturnList()
		{
			//Arrange
			var palletGuid5 = Guid.Parse("00000000-0005-1111-0000-000000000000");
			var palletGuid8 = Guid.Parse("00000000-0008-1111-0000-000000000000");

			var palletGuid2 = Guid.Parse("00000000-0002-1111-0000-000000000000");

			var startDate = TestDates.UtcNow.AddDays(-2);
			var endDate = TestDates.UtcNow.AddDays(1);
			//Act
			var result = await _virtualPalletRepo.GetVirtualPalletsByTimeAsync(startDate, endDate, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(2, result.Count);
			// Verify that these are the correct pallets
			var palletIds = result.Select(v => v.PalletId).ToList();
			Assert.Contains(palletGuid5, palletIds);
			Assert.Contains(palletGuid8, palletIds);
			// No other pallets outside the range
			Assert.DoesNotContain(palletGuid2, palletIds);
			// Optionally, verify that the dates are within the range
			Assert.All(result, v =>
				Assert.InRange(v.DateMoved, startDate, endDate));
		}

		[Fact]
		public async Task ReturnData_GetVirtualPalletByIdAsync_GiveBackProperData()
		{
			//Arrange
			var vpId1 = Guid.Parse("22222222-1111-2222-1111-111111111111");
			//Act
			var result = await _virtualPalletRepo.GetVirtualPalletByIdAsync(vpId1, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.IsType<VirtualPallet>(result);
		}
	}
}
