using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.IntegrationTest.PalletTestsIntegration
{
	public class AddPalletIntegrationTests : PalletIntergrationCommand
	{
		[Fact]
		public void CreateNewPallet_AddPalletReceipt_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ReceiptId = 100,
				ProductsOnPallet = [new()
					{
						ProductId = 1,
						Quantity = 10,						
					}
				]
			};
			//Act
			var result = _palletService.AddPalletReceipt(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.NotNull(pallet);
			Assert.Equal(10, pallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(ReasonMovement.Received, pallet.PalletMovements.First(x=>x.PalletId == result).Reason);
			//Assert.Equal(ReasonMovement.Received, pallet.PalletMovements.First(x=>x.ProductId==1).Reason);
			//Assert.Equal(ReasonMovement.Received, pallet.PalletMovements
				//.Where(md => md.PalletMovementDetails.Any(md => md.ProductId == 1)).);
				//.First(x=>x.ProductId==1).Reason);

		}
		[Fact]
		public void NoProduct_AddPalletReceipt_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ReceiptId = 100,
			};
			//Act&Assert
			var ex = Assert.Throws<ValidationException>(() => _palletService.AddPalletReceipt(palletDTO));
			Assert.Contains("Paleta musi zawierać towar/y", ex.Message);

		}
		[Fact]
		public void InCorrectData_AddPalletReceipt_ThrowException()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ReceiptId = 100,
				ProductsOnPallet = [new() { ProductId = 1, }]
			};
			//Act&Assert
			var exMessage = Assert.Throws<ValidationException>(() => _palletService.AddPalletReceipt(palletDTO));
			Assert.Contains("Ilość", exMessage.Message);
		}
		[Fact]
		public async Task CreateNewPallet_AddPalletReceiptAsync_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ReceiptId = 100,
				ProductsOnPallet = [new() { ProductId = 1, Quantity = 1, }]
			};
			//Act
			var result = await _palletService.AddPalletReceiptAsync(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.Equal(1, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public async Task NoProduct_AddPalletReceiptAsync_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ReceiptId = 100,
			};
			//Act&Assert
			var ex =await Assert.ThrowsAsync<ValidationException>(() => _palletService.AddPalletReceiptAsync(palletDTO));
			Assert.Contains("Paleta musi zawierać towar/y", ex.Message);

		}
		[Fact]
		public async Task InCorrectData_AddPalletReceiptAsync_ReturnException()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ReceiptId = 100,
				ProductsOnPallet = [new() { Quantity = 1, }]
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(() => _palletService.AddPalletReceiptAsync(palletDTO));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
		}
		[Fact]
		public void CreateNewPallet_CreatePickingPallet_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 200,
				Status = PalletStatus.Available,
				ProductsOnPallet = [new()
					{
						ProductId = 1,
						Quantity = 10,
					}
				]
			};
			//Act
			var result = _palletService.CreatePickingPallet(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.NotNull(pallet);
			Assert.Equal(10, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public void NoProduct_CreatePickingPallet_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 100,
			};
			//Act&Assert
			var ex = Assert.Throws<ValidationException>(() => _palletService.CreatePickingPallet(palletDTO));
			Assert.Contains("Paleta musi zawierać towar/y", ex.Message);

		}
		[Fact]
		public void InCorrectData_CreatePickingPallet_ThrowException()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 200,
				ProductsOnPallet = [new() { ProductId = 1, }]
			};
			//Act&Assert
			var exMessage = Assert.Throws<ValidationException>(() => _palletService.CreatePickingPallet(palletDTO));
			Assert.Contains("Ilość", exMessage.Message);
		}
		[Fact]
		public void NoProduct_CreatePickingPallet_ThrowException()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 200,
				ProductsOnPallet = [new() { ProductId = 1, }]
			};
			//Act&Assert
			var exMessage = Assert.Throws<ValidationException>(() => _palletService.CreatePickingPallet(palletDTO));
			Assert.Contains("Ilość", exMessage.Message);
		}
		[Fact]
		public async Task CreateNewPallet_CreatePickingPalletAsync_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 200,
				ProductsOnPallet = [new()
					{
						ProductId = 1,
						Quantity = 10,
					}
				]
			};
			//Act
			var result = await _palletService.CreatePickingPalletAsync(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.Equal(10, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public async Task NoProduct_CreatePickingPalletAsync_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 100,
			};
			//Act&Assert
			var ex =await Assert.ThrowsAsync<ValidationException>(() => _palletService.CreatePickingPalletAsync(palletDTO));
			Assert.Contains("Paleta musi zawierać towar/y", ex.Message);

		}
		[Fact]
		public async Task InCorrectData_CreatePickingPalletAsync_ReturnException()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				IssueId = 200,
				ProductsOnPallet = [new() { Quantity = 1, }]
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(() => _palletService.CreatePickingPalletAsync(palletDTO));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
		}
	}
}
