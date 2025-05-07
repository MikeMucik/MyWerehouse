using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.PalletsTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewPalletTests
	{
		private readonly PalletRepo _palletRepo;
		public ViewPalletTests(QuerryTestFixture fixture)
		{
			var _context = fixture.Context;
			_palletRepo = new PalletRepo(_context);
		}
		[Fact]
		public void GetPallet_GetPalletWithProducts_ReturnPalletWithProduct()
		{
			//Arrange
			var paletId = "Q1000";
			//Act
			var result = _palletRepo.GetPalletWithProducts(paletId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(10, result.ProductsOnPallet.First(p => p.Id == 1).Quantity);
			Assert.Equal(new DateTime(2026, 2, 2, 0, 0, 0), result.ProductsOnPallet.First(p => p.Id == 1).DateAdded);
		}
		[Fact]
		public void GetPallet_GetPalletWithHistory_ReturnPalletWithHistory()
		{
			//Arrange
			var paletId = "Q1000";
			//Act
			var result = _palletRepo.GetPalletWithHistory(paletId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.PalletMovements.First(p => p.Id == 1).Quantity);
			Assert.Equal(new DateTime(2025, 2, 2, 0, 0, 0), result.PalletMovements.First(p => p.Id == 1).MovementDate);
		}
		[Fact]
		public void SearchPallets_FindPalletsByProductId_ReturnList()
		{
			//Arrange
			var productId = new PalletSearchFilter
			{
				ProductId = 10
			};
			//Act
			var result = _palletRepo.GetPalletsByBasedFilter(productId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
			Assert.Contains(result, p => p.Id == "Q1000");
		}
		[Fact]
		public void SearchPallets_FindPalletsByLocationId_ReturnList()
		{
			//Arrange
			var locationId = new PalletSearchFilter
			{
				LocationId = 1
			};
			//Act
			var result = _palletRepo.GetPalletsByBasedFilter(locationId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("Q1000", result.First().Id);
		}
		[Fact]
		public void SearchPallets_GetPalletsByClientFilter_ReturnList()
		{
			//Arrange
			var ClientId = new PalletSearchFilter
			{
				ClientIdIn = 10
			};
			//Act
			var result = _palletRepo.GetPalletsByClientFilter(ClientId);
			//Assert
			Assert.NotNull(result);

		}
	}
}
