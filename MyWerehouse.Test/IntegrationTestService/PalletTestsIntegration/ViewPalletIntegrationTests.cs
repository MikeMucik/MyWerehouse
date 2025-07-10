using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.PalletTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewPalletIntegrationTests(QuerryTestFixture fixture) : PalletIntegrationView(fixture)
	{
		[Fact]
		public void ShowDataToEdit_GetPalletToEdit_ReturnUpdatePalletDTO()
		{
			//Arrange
			var palletId = "Q1001";
			//Act
			var result = _pallletService.GetPalletToEdit(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.ProductsOnPallet.Count);
			Assert.Equal(1, result.LocationId);
			Assert.Equal(PalletStatus.OnHold, result.Status);
			Assert.Equal(1, result.ReceiptId);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(new DateTime(2020,1,1), result.DateReceived);
			var product1 = result.ProductsOnPallet.Single(p => p.ProductId == 10);
			Assert.Equal(100, product1.Quantity);
			Assert.Equal(new DateOnly(2025,2,2), product1.BestBefore);
			var product2 = result.ProductsOnPallet.Single(p => p.ProductId == 11);
			Assert.Equal(200, product2.Quantity);
		}
		[Fact]
		public async Task ShowDataToEdit_GetPalletToEditAsync_ReturnUpdatePalletDTO()
		{
			//Arrange
			var palletId = "Q1001";
			//Act
			var result =await _pallletService.GetPalletToEditAsync(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.ProductsOnPallet.Count);
			Assert.Equal(1, result.LocationId);
			Assert.Equal(PalletStatus.OnHold, result.Status);
			Assert.Equal(1, result.ReceiptId);
			Assert.Equal(2, result.IssueId);
			Assert.Equal(new DateTime(2020, 1, 1), result.DateReceived);
			var product1 = result.ProductsOnPallet.Single(p => p.ProductId == 10);
			Assert.Equal(100, product1.Quantity);
			Assert.Equal(new DateOnly(2025, 2, 2), product1.BestBefore);
			var product2 = result.ProductsOnPallet.Single(p => p.ProductId == 11);
			Assert.Equal(200, product2.Quantity);
		}
		[Fact]
		public void ShowHistory_ShowHistoryPallet_ReturnData()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			var result = _pallletService.ShowHistoryPallet(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.PalletMovementsDTO);
			Assert.Equal(2, result.PalletMovementsDTO.Count());
			var move1 = result.PalletMovementsDTO.Single(x=>x.Id ==5);
			Assert.NotNull(move1);
			var move2 = result.PalletMovementsDTO.Single(x=>x.Id ==1);
			Assert.NotNull(move2);
			Assert.Equal(3, move1.DestinationLocationId);
			Assert.Equal(ReasonMovement.ManualMove, move1.Reason);
			Assert.Equal(new DateTime(2025,2,2), move1.MovementDate);
			Assert.Equal(2, move2.PalletMovementDetailsDTO.Count());
			Assert.Equal(100, move2.PalletMovementDetailsDTO.Single(x=>x.ProductId==10).QuantityChange);
		}
		[Fact]
		public async Task ShowHistory_ShowHistoryPalletAsync_ReturnData()
		{
			//Arrange
			var palletId = "Q1000";
			//Act
			var result =await _pallletService.ShowHistoryPalletAsync(palletId);
			//Assert
			Assert.NotNull(result);
			Assert.NotNull(result.PalletMovementsDTO);
			Assert.Equal(2, result.PalletMovementsDTO.Count());
			var move1 = result.PalletMovementsDTO.Single(x => x.Id == 5);
			var move2 = result.PalletMovementsDTO.Single(x => x.Id == 1);
			Assert.Equal(3, move1.DestinationLocationId);
			Assert.Equal(ReasonMovement.ManualMove, move1.Reason);
			Assert.Equal(new DateTime(2025, 2, 2), move1.MovementDate);
			Assert.Equal(2, move2.PalletMovementDetailsDTO.Count());
			Assert.Equal(100, move2.PalletMovementDetailsDTO.Single(x => x.ProductId == 10).QuantityChange);
		}
	}
}
