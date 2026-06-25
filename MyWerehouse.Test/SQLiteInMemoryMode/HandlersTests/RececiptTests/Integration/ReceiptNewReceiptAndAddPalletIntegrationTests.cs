using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Application.Receipts.Commands.CreateReceipt;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.RececiptTests.Integration
{
	public class ReceiptNewReceiptAndAddPalletIntegrationTests : TestBase
	{
		private Client CreateClient()
		{
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
			return new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
		}
		private Location CreateLocation(int id, int position)
		{
			return new Location
			{
				Id = id,
				Bay = 1,
				Aisle = 1,
				Height = 1,
				Position = position
			};
		}
		
		[Fact]
		public async Task CreateReceiptPlan_ShouldCreateReceipt_WhenProperData()
		{
			//Arrange
			var client = CreateClient();
			var location = CreateLocation(1, 1);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.SaveChanges();
			//Act
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = client.Id,
				ReceiptDateTime = DateTime.UtcNow,
				PerformedBy = "user",
				RampNumber = 1
			};
			var result = await Mediator.Send(new CreateReceiptPlanCommand(newPalletDto));
			//Assert
			Assert.NotNull(result);
			var receipt = DbContext.Receipts.FirstOrDefault(x => x.ClientId == client.Id);
			Assert.NotNull(receipt);
			Assert.Equal(ReceiptStatus.Planned, receipt.ReceiptStatus);
			Assert.Equal("user", receipt.PerformedBy);			
		}

		[Fact]
		public async Task CreateReceiptPlan_ReturnValidationError_WhenNoUser()
		{
			//Arrange
			var client = CreateClient();
			var location = CreateLocation(1, 1);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.SaveChanges();
			//Act&Assert
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = client.Id,
				ReceiptDateTime = DateTime.UtcNow,
				//PerformedBy = "user",
				RampNumber = 1
			};
			var ex = await Assert.ThrowsAsync <ValidationException>(()=> Mediator.Send(new CreateReceiptPlanCommand(newPalletDto)));
			Assert.Contains("Użytkownik wymagany.", ex.Message);
			
		}
		[Fact]
		public async Task CreateReceiptPlan_ReturnValidationInfo_WhenNotExistingClient()
		{
			//Arrange
			var client = CreateClient();// a tu jest CLient.Id = 1
			var location = CreateLocation(1, 1);
			DbContext.Locations.Add(location);
			DbContext.Clients.Add(client);
			DbContext.SaveChanges();
			//Act
			var newPalletDto = new CreateReceiptPlanDTO
			{
				ClientId = 2,
				ReceiptDateTime = DateTime.UtcNow,
				PerformedBy = "user",
				RampNumber = 1
			};
			//Act&Assert
			var ex = await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(new CreateReceiptPlanCommand(newPalletDto)));
			Assert.Contains("Klient nie istnieje.", ex.Message);
		}
		
	}
}
