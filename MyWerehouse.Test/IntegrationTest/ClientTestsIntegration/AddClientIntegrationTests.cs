using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;

namespace MyWerehouse.Test.IntegrationTest.ClientTestsIntegration
{
	public class AddClientIntegrationTests : ClientIntegrationCommand
	{
		[Fact]
		public void ProperData_AddClient_AddedToBase()
		{
			//Arrange
			var address = new AddressDTO
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var client = new AddClientDTO
			{
				Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<AddressDTO> { address },
				Description = "description",
			};
			//Act
			var result = _clientService.AddClient(client);
			//Assert			
			var resultClient = _context.Clients.FirstOrDefault(c => c.Name == client.Name);
			Assert.NotNull(resultClient);
			Assert.Equal(client.Email, resultClient.Email);
			var resultAddress = _context.Addresses.Where(a => a.ClientId == result);
			Assert.NotNull(resultAddress);
			Assert.Equal("Wiejska", resultAddress.First().StreetName);
		}
		[Fact]
		public void ProperDataTwoAdresses_AddClient_AddedToBase()
		{
			//Arrange
			var address = new AddressDTO
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address1 = new AddressDTO
			{
				City = "Warsaw",
				Country = "USA",
				PostalCode = "00-999",
				StreetName = "Country",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var client = new AddClientDTO
			{
				Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = [address, address1],
				Description = "description",
			};
			//Act
			var result = _clientService.AddClient(client);
			//Assert			
			var resultClient = _context.Clients.FirstOrDefault(c => c.Name == client.Name);
			Assert.NotNull(resultClient);
			Assert.Equal(client.Email, resultClient.Email);
			var resultAddress = _context.Addresses.Where(a => a.ClientId == result);
			Assert.NotNull(resultAddress);
			Assert.Contains(resultClient.Addresses, a => a.StreetName == address.StreetName);
			Assert.Contains(resultClient.Addresses, a => a.StreetName == address1.StreetName);
		}
		[Fact]
		public void NotProperDataPostalCode_AddClient_NoAddedToBase()
		{
			//Arrange
			var address = new AddressDTO
			{
				City = "Warsaw",
				Country = "Poland",
				//PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var client = new AddClientDTO
			{
				Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<AddressDTO> { address },
				Description = "description",
			};
			//Act&Assert
			var exceptionMessage = Assert.Throws<ValidationException>(() => _clientService.AddClient(client));
			Assert.Contains("numer pocztowy", exceptionMessage.Message);
		}
		[Fact]
		public void NotProperDataName_AddClient_NoAddedToBase()
		{
			//Arrange
			var address = new AddressDTO
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var client = new AddClientDTO
			{
				//Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<AddressDTO> { address },
				Description = "description",
			};
			//Act&Assert
			var exceptionMessage = Assert.Throws<ValidationException>(() => _clientService.AddClient(client));
			Assert.Contains("nazwa", exceptionMessage.Message);
		}
	}
}
