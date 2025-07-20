using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Test.IntegrationTestService.ReceiptTestsIntegration
{
	public class AddReceiptIntegrationTest : ReceiptIntegrationCommand
	{
		
		[Fact]
		public async Task CreateReceipt_CreateReceiptPlanAsync_OpenNewReceipt()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var client = new Client
			{
				Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<Address> { address },
				Description = "description",
			};

			_context.Clients.Add(client);

			var newReceipt = new CreateReceiptPlanDTO
			{
				ClientId = 1,
				PerformedBy = "U0010",
			};
			var receiptId = await _receiptService.CreateReceiptPlanAsync(newReceipt);
			//Assert
			var result = _context.Receipts.FirstOrDefault(r => r.Id == receiptId);
			Assert.NotNull(result);
			Assert.Equal(ReceiptStatus.Planned, result.ReceiptStatus);
			Assert.Equal("U0010", result.PerformedBy);
		}
		
		
		//[Fact]
		//public async Task AddPallet_AddPalletToReceiptAsync_AddToCollection()
		//{
		//	//Arrange
		//	var location = new Location
		//	{
		//		Id = 1,
		//		Aisle = 0,
		//		Bay = 0,
		//		Height = 0,
		//		Position = 0
		//	};
		//	var receipt = new Receipt
		//	{
		//		//Id = 1,
		//		ClientId = 1,
		//		ReceiptStatus = ReceiptStatus.Planned,
		//		ReceiptDateTime = new DateTime(2025, 6, 6),
		//		PerformedBy = "U0010"
		//	};
		//	_context.Receipts.Add(receipt);
		//	_context.Locations.Add(location);
		//	_context.SaveChanges();

		//	//Act
		//	var receiptId = 1;
		//	var palletDTO = new CreatePalletReceiptDTO
		//	{
		//		//ReceiptId = 1,
		//		ProductsOnPallet = [new()
		//				{
		//					ProductId = 1,
		//					Quantity = 10,
		//				}
		//		]
		//	};
		//	var addedPalletId = await _receiptService.AddPalletToReceiptAsync(receiptId, palletDTO);
		//	//Assert
		//	Assert.NotNull(addedPalletId);
		//	var pallet = _context.Pallets.Find(addedPalletId);
		//	Assert.NotNull(pallet);
		//	Assert.Equal(10, pallet.ProductsOnPallet.First().Quantity);
		//	Assert.Equal(ReasonMovement.Received, pallet.PalletMovements.First(x => x.PalletId == addedPalletId).Reason);
		//}

	}
}
//var location = new Location
//{
//	Id = 1,
//	Aisle = 0,
//	Bay = 0,
//	Height = 0,
//	Position = 0
//};

//_context.Locations.Add(location);
//var palletDTO = new CreatePalletReceiptDTO
//{
//	ReceiptId = 100,
//	ProductsOnPallet = [new()
//		{
//			ProductId = 1,
//			Quantity = 10,
//		}
//	]
//};
//var palletDTO1 = new CreatePalletReceiptDTO
//{
//	ReceiptId = 100,
//	ProductsOnPallet = [new()
//		{
//			ProductId = 10,
//			Quantity = 100,
//		}
//	]
//};
//Act
//var newReceipt = new AddReceiptDTO
//{
//	ClientId = 1,
//	Pallets = [  palletDTO, palletDTO1 ],
//	PerformedBy = "U0001",
//	ReceiptDateTime = DateTime.Now,
//};