using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Unit
{
	public class ManualPickingUnitTests : TestBase
	{
		//[Fact]
		//public async Task DoManualPicking_PalletNotFound_ThrowsArgumentException()
		//{
		//	// Arrange
		//	var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//	var mockMapper = new Mock<IMapper>();
		//	var locationRepoMock = new Mock<ILocationRepo>();
		//	var palletRepo = new Mock<IPalletRepo>();
		//	var issueRepoMock = new Mock<IIssueRepo>();
		//	var historyServiceMock = new Mock<IHistoryService>();
		//	var inventoryRepoMock = new Mock<IInventoryRepo>();
		//	var palletService = new Mock<IPalletService>();

		//	var service = new PickingPalletService(
		//		pickingRepoMock.Object,
		//		mockMapper.Object,
		//		DbContext,
		//		locationRepoMock.Object,
		//		palletRepo.Object,
		//		issueRepoMock.Object,
		//		historyServiceMock.Object,
		//		palletService.Object
		//	);
		//	palletRepo.Setup(r => r.GetPalletByIdAsync("P123"))
		//		.ReturnsAsync((Pallet)null);

		//	// Act + Assert
		//	await Assert.ThrowsAsync<ArgumentException>(() => service.DoManualPickingAsync("P123", null, "user1"));
		//}
		[Fact]
		public async Task PrepareManualPicking_PalletNotFound_ThrowsArgumentException()
		{
			// Arrange
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var mockMapper = new Mock<IMapper>();
			var locationRepoMock = new Mock<ILocationRepo>();
			var palletRepo = new Mock<IPalletRepo>();
			var issueRepoMock = new Mock<IIssueRepo>();
			var historyServiceMock = new Mock<IHistoryService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var palletService = new Mock<IPalletService>();

			var service = new PickingPalletService(
				pickingRepoMock.Object,
				//mockMapper.Object,
				DbContext,
				locationRepoMock.Object,
				palletRepo.Object,
				issueRepoMock.Object,
				historyServiceMock.Object,
				palletService.Object
			);
			palletRepo.Setup(r => r.GetPalletByIdAsync("P123"))
				.ReturnsAsync((Pallet)null);

			// Act
			var result =await service.PrepareManualPickingAsync("P123");
			// Assert
			Assert.False(result.Success);
			Assert.Contains($"Brak palety P123 na stanie.", result.Message);
		}

		//[Fact]
		//public async Task DoManualPicking_PalletNotInToPicking_ReturnsError()
		//{
		//	// Arrange
		//	var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//	var mockMapper = new Mock<IMapper>();
		//	var locationRepoMock = new Mock<ILocationRepo>();
		//	var palletRepo = new Mock<IPalletRepo>();
		//	var issueRepoMock = new Mock<IIssueRepo>();
		//	var historyServiceMock = new Mock<IHistoryService>();
		//	var inventoryRepoMock = new Mock<IInventoryRepo>();
		//	var palletService = new Mock<IPalletService>();

		//	var service = new PickingPalletService(
		//		pickingRepoMock.Object,
		//		mockMapper.Object,
		//		DbContext,
		//		locationRepoMock.Object,
		//		palletRepo.Object,
		//		issueRepoMock.Object,
		//		historyServiceMock.Object,
		//		palletService.Object
		//	);
		//	var pallet = new Pallet
		//	{
		//		Id = "P123",
		//		Status = PalletStatus.Available,
		//		ProductsOnPallet = new List<ProductOnPallet>
		//	{
		//		new() { ProductId = 1, Quantity = 10 }
		//	}
		//	};
		//	palletRepo.Setup(r => r.GetPalletByIdAsync("P123")).ReturnsAsync(pallet);

		//	// Act
		//	var result = await service.DoManualPickingAsync("P123", null, "user1");

		//	// Assert
		//	Assert.False(result.Success);
		//	Assert.Equal("Paleta P123 nie jest w pickingu, zmień status.", result.Message);
		//}
		[Fact]
		public async Task PrepareManualPicking_PalletNotInToPicking_ReturnsError()
		{
			// Arrange
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var mockMapper = new Mock<IMapper>();
			var locationRepoMock = new Mock<ILocationRepo>();
			var palletRepo = new Mock<IPalletRepo>();
			var issueRepoMock = new Mock<IIssueRepo>();
			var historyServiceMock = new Mock<IHistoryService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var palletService = new Mock<IPalletService>();

			var service = new PickingPalletService(
				pickingRepoMock.Object,
				//mockMapper.Object,
				DbContext,
				locationRepoMock.Object,
				palletRepo.Object,
				issueRepoMock.Object,
				historyServiceMock.Object,
				palletService.Object
			);
			var pallet = new Pallet
			{
				Id = "P123",
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
			{
				new() { ProductId = 1, Quantity = 10 }
			}
			};
			palletRepo.Setup(r => r.GetPalletByIdAsync("P123")).ReturnsAsync(pallet);

			// Act
			var result = await service.PrepareManualPickingAsync("P123");

			// Assert
			Assert.False(result.Success);
			Assert.Equal("Paleta P123 nie jest w pickingu, zmień status.", result.Message);
		}
		//[Fact]
		//public async Task DoManualPicking_WithIssueIdNotFound_ThrowsArgumentException()
		//{
		//	// Arrange

		//	var pallet = new Pallet
		//	{
		//		Id = "P123",
		//		Status = PalletStatus.ToPicking,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 1, Quantity = 5 } }
		//	};
		//	var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//	var mockMapper = new Mock<IMapper>();
		//	var locationRepoMock = new Mock<ILocationRepo>();
		//	var palletRepo = new Mock<IPalletRepo>();
		//	var issueRepoMock = new Mock<IIssueRepo>();
		//	var historyServiceMock = new Mock<IHistoryService>();
		//	var inventoryRepoMock = new Mock<IInventoryRepo>();
		//	var palletService = new Mock<IPalletService>();

		//	var service = new PickingPalletService(
		//		pickingRepoMock.Object,
		//		mockMapper.Object,
		//		DbContext,
		//		locationRepoMock.Object,
		//		palletRepo.Object,
		//		issueRepoMock.Object,
		//		historyServiceMock.Object,
		//		palletService.Object
		//	);
		//	palletRepo.Setup(r => r.GetPalletByIdAsync("P123")).ReturnsAsync(pallet);
		//	issueRepoMock.Setup(r => r.GetIssueByIdAsync(1001)).ReturnsAsync((Issue)null);

		//	//// Act + Assert
		//	//await Assert.ThrowsAsync<ArgumentException>(() => service.DoManualPicking("P123", 1001, "user1"));

		//	// Act
		//	var result = await service.DoManualPickingAsync("P123", 1001, "user1");

		//	// Assert
		//	Assert.False(result.Success);
		//	Assert.Contains("Brak zamówienia o numerze 1001", result.Message);
		//}
		[Fact]
		public async Task ExecuteManualPickingAsync_WithIssueIdNotFound_ThrowsArgumentException()
		{
			// Arrange

			var pallet = new Pallet
			{
				Id = "P123",
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 1, Quantity = 5 } }
			};
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var mockMapper = new Mock<IMapper>();
			var locationRepoMock = new Mock<ILocationRepo>();
			var palletRepo = new Mock<IPalletRepo>();
			var issueRepoMock = new Mock<IIssueRepo>();
			var historyServiceMock = new Mock<IHistoryService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var palletService = new Mock<IPalletService>();

			var service = new PickingPalletService(
				pickingRepoMock.Object,
				//mockMapper.Object,
				DbContext,
				locationRepoMock.Object,
				palletRepo.Object,
				issueRepoMock.Object,
				historyServiceMock.Object,
				palletService.Object
			);
			palletRepo.Setup(r => r.GetPalletByIdAsync("P123")).ReturnsAsync(pallet);
			//var issueId =
				issueRepoMock.Setup(r => r.GetIssueByIdAsync(1001)).ReturnsAsync((Issue)null);

			//// Act + Assert
			//await Assert.ThrowsAsync<ArgumentException>(() => service.DoManualPicking("P123", 1001, "user1"));

			// Act
			var result = await service.ExecuteManualPickingAsync("P123", 1001, "userId");

			// Assert
			Assert.False(result.Success);
			Assert.Contains("Zamówienie o numerze 1001 nie zostało znalezione.", result.Message);
		}
		//[Fact]
		//public async Task DoManualPicking_WithException_RollsBackAndReturnsError()
		//{
		//	// Arrange
		//	var pickingRepoMock = new Mock<IPickingPalletRepo>();
		//	var mockMapper = new Mock<IMapper>();
		//	var locationRepoMock = new Mock<ILocationRepo>();
		//	var palletRepo = new Mock<IPalletRepo>();
		//	var issueRepoMock = new Mock<IIssueRepo>();
		//	var historyServiceMock = new Mock<IHistoryService>();
		//	var inventoryRepoMock = new Mock<IInventoryRepo>();
		//	var palletService = new Mock<IPalletService>();

		//	var service = new PickingPalletService(
		//		pickingRepoMock.Object,
		//		mockMapper.Object,
		//		DbContext,
		//		locationRepoMock.Object,
		//		palletRepo.Object,
		//		issueRepoMock.Object,
		//		historyServiceMock.Object,
		//		palletService.Object
		//	);
		//	var pallet = new Pallet
		//	{
		//		Id = "P123",
		//		Status = PalletStatus.ToPicking,
		//		ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 1, Quantity = 5 } }
		//	};
		//	palletRepo.Setup(r => r.GetPalletByIdAsync("P123")).ReturnsAsync(pallet);
		//	issueRepoMock.Setup(r => r.GetIssueByIdAsync(1001)).ReturnsAsync(new Issue { Id = 1001 });
		//	pickingRepoMock.Setup(r => r.GetAllocationsByIssueIdProductIdAsync(1001, 1))
		//		.ThrowsAsync(new Exception("DB Error"));

		//	// Act
		//	var result = await service.DoManualPickingAsync("P123", 1001, "user1");

		//	// Assert
		//	Assert.False(result.Success);
		//	Assert.Contains("Bład podczas ręcznej kompletacji", result.Message);
		//}

		[Fact]
		public async Task ExecuteManualPickingAsync_WithException_RollsBackAndReturnsError()
		{
			// Arrange
			var pickingRepoMock = new Mock<IPickingPalletRepo>();
			var mockMapper = new Mock<IMapper>();
			var locationRepoMock = new Mock<ILocationRepo>();
			var palletRepo = new Mock<IPalletRepo>();
			var issueRepoMock = new Mock<IIssueRepo>();
			var historyServiceMock = new Mock<IHistoryService>();
			var inventoryRepoMock = new Mock<IInventoryRepo>();
			var palletService = new Mock<IPalletService>();

			var service = new PickingPalletService(
				pickingRepoMock.Object,
				//mockMapper.Object,
				DbContext,
				locationRepoMock.Object,
				palletRepo.Object,
				issueRepoMock.Object,
				historyServiceMock.Object,
				palletService.Object
			);
			var pallet = new Pallet
			{
				Id = "P123",
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet> { new() { ProductId = 1, Quantity = 5 } }
			};
			palletRepo.Setup(r => r.GetPalletByIdAsync("P123")).ReturnsAsync(pallet);
			issueRepoMock.Setup(r => r.GetIssueByIdAsync(1001)).ReturnsAsync(new Issue { Id = 1001 });
			pickingRepoMock.Setup(r => r.GetAllocationsByIssueIdProductIdAsync(1001, 1))
				.ThrowsAsync(new Exception("DB Error"));

			// Act
			var result = await service.ExecuteManualPickingAsync("P123", 1001, "user");

			// Assert
			Assert.False(result.Success);
			Assert.Contains("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.", result.Message);
		}
	}
}
