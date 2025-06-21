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
				ProductsOnPallet = [new()
					{												
						ProductId = 1,
						Quantity = 1,
					}
				]
			};
			//Act
			var result = _palletService.AddPalletReceipt(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.NotNull(pallet);
			Assert.Equal(1, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public void InCorrectData_AddPalletReceipt_ThrowException()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new(){ProductId = 1,}]
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
				ProductsOnPallet = [new(){ProductId = 1,Quantity = 1,}]
			};
			//Act
			var result =await _palletService.AddPalletReceiptAsync(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.Equal(1, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public async Task InCorrectData_AddPalletReceiptAsync_ReturnException()
		{
			//Arrange			
			var palletDTO = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() {  Quantity = 1, }]
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(()=> _palletService.AddPalletReceiptAsync(palletDTO));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
		}
		[Fact]
		public void CreateNewPallet_AddPalletPicking_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				Status = PalletStatus.Available,
				ProductsOnPallet = [new()
					{											
						ProductId = 1,
						Quantity = 10,
					}
				]
			};
			//Act
			var result = _palletService.AddPalletPicking(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.NotNull(pallet);
			Assert.Equal(10, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public void InCorrectData_AddPalletPicking_ThrowException()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				ProductsOnPallet = [new() { ProductId = 1, }]
			};
			//Act&Assert
			var exMessage = Assert.Throws<ValidationException>(() => _palletService.AddPalletPicking(palletDTO));
			Assert.Contains("Ilość", exMessage.Message);
		}
		[Fact]
		public async Task CreateNewPallet_AddPalletPickingAsync_AddToList()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				ProductsOnPallet = [new()
					{						
						ProductId = 1,
						Quantity = 10,
					}
				]
			};
			//Act
			var result = await _palletService.AddPalletPickingAsync(palletDTO);
			//Assert
			Assert.NotNull(result);
			var pallet = _palletRepo.GetPalletById(result);
			Assert.Equal(10, pallet.ProductsOnPallet.First().Quantity);
		}
		[Fact]
		public async Task InCorrectData_AddPalletPickingAsync_ReturnException()
		{
			//Arrange			
			var palletDTO = new CreatePalletPickingDTO
			{
				ProductsOnPallet = [new() { Quantity = 1, }]
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(() => _palletService.AddPalletPickingAsync(palletDTO));
			Assert.Contains("Produkt na palecie musi mieć numer produktu", ex.Message);
		}
	}
}
