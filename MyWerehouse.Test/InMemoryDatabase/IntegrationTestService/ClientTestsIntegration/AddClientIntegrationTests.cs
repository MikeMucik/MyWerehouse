using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{
	public class AddClientIntegrationTests : ClientIntegrationCommand
	{
		[Fact]
		public async Task AddClient_ShouldAddClient_WhenValidData()
		{
			//Arrange
			var address = new AddAddressDTO
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
				Addresses = new List<AddAddressDTO> { address },
				Description = "description",
			};
			//Act
			var result = await _clientService.AddClientAsync(client);
			//Assert			
			var resultClient = _context.Clients.FirstOrDefault(c => c.Name == client.Name);
			Assert.NotNull(resultClient);
			Assert.Equal(client.Email, resultClient.Email);
			var resultAddress = _context.Addresses.Where(a => a.ClientId == result.Result);
			Assert.NotNull(resultAddress);
			Assert.Equal("Wiejska", resultAddress.First().StreetName);
		}
		[Fact]
		public async Task AddClient_ShouldAddClinet_WhenValidDataTwoAdresses()
		{
			//Arrange
			var address = new AddAddressDTO
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address1 = new AddAddressDTO
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
			var result = await _clientService.AddClientAsync(client);
			//Assert			
			var resultClient = _context.Clients.FirstOrDefault(c => c.Name == client.Name);
			Assert.NotNull(resultClient);
			Assert.Equal(client.Email, resultClient.Email);
			var resultAddress = _context.Addresses.Where(a => a.ClientId == result.Result);
			Assert.NotNull(resultAddress);
			Assert.Contains(resultClient.Addresses, a => a.StreetName == address.StreetName);
			Assert.Contains(resultClient.Addresses, a => a.StreetName == address1.StreetName);
		}
		[Fact]
		public async Task AddClient_ShouldThrowValidationException_WhenNoPostalCode()
		{
			//Arrange
			var address = new AddAddressDTO
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
				Addresses = new List<AddAddressDTO> { address },
				Description = "description",
			};
			//Act&Assert
			var exceptionMessage = await Assert.ThrowsAsync<ValidationException>(() => _clientService.AddClientAsync(client));
			Assert.Contains("numer pocztowy", exceptionMessage.Message);
		}
		[Fact]
		public async Task AddClient_ShouldThrowValidationException_WhenNoName()
		{
			//Arrange
			var address = new AddAddressDTO
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
				Addresses = new List<AddAddressDTO> { address },
				Description = "description",
			};
			//Act&Assert
			var exceptionMessage = await Assert.ThrowsAsync<ValidationException>(() => _clientService.AddClientAsync(client));
			Assert.Contains("nazwa", exceptionMessage.Message);
		}
	}
}