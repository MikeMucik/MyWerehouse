using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PalletsTestsRepoSQLite
{
	[Collection("QueryCollection")]
	public class ViewPalletTests
	{
		private readonly PalletRepo _palletRepo;
		private readonly QueryTestSQLFixture _fixture;
		public ViewPalletTests(QueryTestSQLFixture fixture)
		{
			_fixture = fixture;
			_palletRepo = new PalletRepo(_fixture.DbContext);
		}
		[Fact]
		public async Task GetPallet_GetPalletByIdAsync_ReturnSimplyData()
		{
			//Arrange
			var paletId =  Guid.Parse("00000000-0001-1111-0000-000000000000");
			//Act
			var result =await _palletRepo.GetPalletByIdAsync(paletId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(PalletStatus.Available, result.Status);
			Assert.Equal(1, result.LocationId);
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			Assert.Equal(receiptId1, result.ReceiptId);
		}

		[Fact]
		public async Task GetPallet_GetPalletWithProductsAsync_ReturnPalletWithProduct()
		{
			//Arrange
			var paletId =  Guid.Parse("00000000-0001-1111-0000-000000000000");
			//Act
			var result = await _palletRepo.GetPalletByIdAsync(paletId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(50, result.ProductsOnPallet.First(p => p.Id == 1).Quantity);
			Assert.Equal(new DateTime(2024, 2, 2), result.ProductsOnPallet.First(p => p.Id == 1).DateAdded);
		}

		[Fact]
		public async Task GetMissingFullPallets_ReturnsSingleProductPallet()
		{
			//Arrange
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");
			var fullPallet = 200;
			DateOnly date = new DateOnly(2024,2,2);
			//Act
			var result =await _palletRepo.GetMissingFullPallets(productId2, fullPallet, date, 1, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.Single(result);
			Assert.Contains(result, p => p.PalletNumber == "Q1002");
		}
		[Fact]
		public async Task GetMissingFullPallets_IgnoresPalletContainingDifferentProducts()
		{
			//Arrange
			var productId1 = Guid.Parse("00000000-0000-0000-0001-000000000000");
			DateOnly date = new DateOnly(2024, 2, 2);
			//Act
			var result = await _palletRepo.GetMissingFullPallets(productId1, 50, date, 1, CancellationToken.None);
			//Assert
			Assert.Empty(result);
		}
		[Fact]
		public async Task GetCandidates_ReturnsOnlySingleProductAllocatablePallets()
		{
			//Arrange
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");
			DateOnly date = new DateOnly(2024, 2, 2);
			//Act
			var result = await _palletRepo.GetCandidates(productId2, date, [], CancellationToken.None);
			//Assert
			var candidate = Assert.Single(result);
			Assert.Equal(Guid.Parse("00000000-0003-1111-0000-000000000000"), candidate.PalletId);
			Assert.Equal(200, candidate.Quantity);
		}
		[Fact]
		public async Task GetCandidates_IgnoresExcludedPallets()
		{
			//Arrange
			var productId2 = Guid.Parse("00000000-0000-0000-0002-000000000000");
			var excludedId = Guid.Parse("00000000-0003-1111-0000-000000000000");
			DateOnly date = new DateOnly(2024, 2, 2);
			//Act
			var result = await _palletRepo.GetCandidates(productId2, date, [excludedId], CancellationToken.None);
			//Assert
			Assert.Empty(result);
		}
		[Fact]
		public async Task GetSelectedPallets_PreservesRequestedOrder()
		{
			//Arrange
			var firstId = Guid.Parse("00000000-0003-1111-0000-000000000000");
			var secondId = Guid.Parse("00000000-0001-1111-0000-000000000000");
			//Act
			var result = await _palletRepo.GetSelectedPallets([firstId, secondId], CancellationToken.None);
			//Assert
			Assert.Equal([firstId, secondId], result.Select(p => p.Id));
			Assert.All(result, pallet => Assert.NotEmpty(pallet.ProductsOnPallet));
		}
		[Fact]
		public async Task CheckOccupancyAsync_ReturnPallets_WhenLocationUsed()
		{
			//Arrange
			var location = 1;
			//Act
			var result = await _palletRepo.CheckOccupancyAsync(location, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.PalletNumber == "Q1000" || result.PalletNumber == "Q1001");
		}
	}
}
