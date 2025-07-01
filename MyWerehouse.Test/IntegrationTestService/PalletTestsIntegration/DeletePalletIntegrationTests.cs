using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using Xunit.Sdk;

namespace MyWerehouse.Test.IntegrationTest.PalletTestsIntegration
{
	public class DeletePalletIntegrationTests : PalletIntergrationCommand
	{
		[Fact]
		public void PalletWithNoHistory_DeletePallet_RemovePalletFromList()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			var palletId = "Q1010";
			//Act
			_palletService.DeletePallet(palletId);
			//Assert
			var result = _palletRepo.GetPalletById(palletId);
			Assert.Null(result);
		}
		[Fact]
		public void PalletWithHistory_DeletePallet_ThrowException()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var movement = new PalletMovement
			{
				Id = 1,
				//Quantity = 1,
				LocationId =1,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				//ProductId = 1,
				Reason = ReasonMovement.ManualMove,
			};
			var movement2 = new PalletMovement
			{
				Id = 2,
				//Quantity = 1,
				LocationId = 2,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				//ProductId = 1,
				Reason = ReasonMovement.ManualMove,
			};
			var movementDetails1 = new PalletMovementDetails
			{
				Id = 1,
				PalletMovementId = 1,
				ProductId = 1,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetails
			{
				Id = 2,
				PalletMovementId = 2,
				ProductId = 1,
				Quantity = 1,
			};
			_context.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			_context.Pallets.Add(pallet);
			_context.PalletMovement.AddRange(movement, movement2);
			_context.SaveChanges();
			var palletId = "Q1000";
			//Act&Assert
			var ex = Assert.Throws<InvalidOperationException>(() => _palletService.DeletePallet(palletId));

			Assert.Contains("Palety o numerze", ex.Message);
		}
		[Fact]
		public void IncorrectID_DeletePallet_ThrowException()
		{
			//Arrange
			
			var palletId = "1000";
			//Act&Assert
			var ex = Assert.Throws<ArgumentException>(() => _palletService.DeletePallet(palletId));

			Assert.Contains("Nie ma palety o tym numerze", ex.Message);
		}
		[Fact]
		public async Task PalletWithNoHistory_DeletePalletAsync_RemovePalletFromList()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};		
			_context.Pallets.Add(pallet);			
			_context.SaveChanges();
			var palletId = "Q1010";			//Act
			await _palletService.DeletePalletAsync(palletId);
			//Assert
			var result = _palletRepo.GetPalletById(palletId);
			Assert.Null(result);
		}
		[Fact]
		public async Task PalletWithHistory_DeletePalletAsync_ThrowException()
		{
			//Arrange
			var pallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				ReceiptId = 10,
			};
			var movement = new PalletMovement
			{
				Id = 1,
				//Quantity = 1,
				LocationId = 1,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				//ProductId = 1,
				Reason = ReasonMovement.ManualMove,
			};
			var movement2 = new PalletMovement
			{
				Id = 2,
				//Quantity = 1,
				LocationId = 2,
				MovementDate = DateTime.Now,
				PalletId = pallet.Id,
				//ProductId = 1,
				Reason = ReasonMovement.ManualMove,
			};
			var movementDetails1 = new PalletMovementDetails
			{
				Id = 1,
				PalletMovementId = 1,
				ProductId = 1,
				Quantity = 1,
			};
			var movementDetails2 = new PalletMovementDetails
			{
				Id = 2,
				PalletMovementId = 2,
				ProductId = 1,
				Quantity = 1,
			};
			_context.PalletMovementDetails.AddRange(movementDetails1, movementDetails2);
			_context.PalletMovement.AddRange(movement,movement2);
			_context.Pallets.Add(pallet);
			_context.SaveChanges();
			var palletId = "Q1000";
			//Act&Assert
			var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _palletService.DeletePalletAsync(palletId));

			Assert.Contains("Palety o numerze", ex.Message);
		}
		[Fact]
		public async Task IncorrectID_DeletePalletAsync_ThrowException()
		{
			//Arrange
			var palletId = "1000";
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ArgumentException>(() => _palletService.DeletePalletAsync(palletId));

			Assert.Contains("Nie ma palety o numerze", ex.Message);
		}
	}
}
