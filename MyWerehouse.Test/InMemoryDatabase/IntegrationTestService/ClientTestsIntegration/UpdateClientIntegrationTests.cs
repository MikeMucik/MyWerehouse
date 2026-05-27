using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{
	public class UpdateClientIntegrationTests : ClientIntegrationCommand
	{	
		[Fact]
		public async Task UpdateClient_ShouldChangeData_WhenDataValid()
		{
			//Arrange			
			var address = new Address
			{
				Id = 10,
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 10,
			};
			var updatingClient = new Client
			{
				Id = 10,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new List<Address> { address }
			};
			_context.Addresses.Add(address);
			_context.Clients.Add(updatingClient);
			_context.SaveChanges();
			//Act
			var addressU = new EditAddressDTO
			{
				Id = 10,
				Country = "Silesia",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
			};
			var id = 10;
			var updatedClient = new UpdateClientDTO
			{
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};			
				await _clientService.UpdateClientAsync(id,updatedClient);			
			//Assert			
				var result = _context.Clients
					.Include(x => x.Addresses)
					.FirstOrDefault(x => x.Id == updatingClient.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedClient.Name, result.Name);
				Assert.Equal(updatedClient.Addresses.First().Country, result.Addresses.First().Country);
				Assert.Equal(updatedClient.Addresses.First().City, result.Addresses.First().City);			
		}
		[Fact]
		public async Task UpdateClient_ShouldThrowValidationException_WhenAddresHasNoPhoneNumber()
		{
			//Arrange
			var address = new Address
			{
				Id = 20,
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 20,
			};
			var updatingClient = new Client
			{
				Id = 20,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new List<Address> { address }

			};
			_context.Addresses.Add(address);
			_context.Clients.Add(updatingClient);
			_context.SaveChanges();
			//Act&Assert
			var addressU = new EditAddressDTO
			{
				Id = 20,
				Country = "Silesia",
				City = "test",
				Region = "test",
				//Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
			};
			var id = 20;
			var updatedClient = new UpdateClientDTO
			{
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};					
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(id, updatedClient));
				Assert.Contains("tele", ex.Message);			
		}
		[Fact]
		public async Task UpdateClient_ShouldThrowValidationException_WhenAddresHasNoEmail()
		{
			//Arrange			
			var address = new Address
			{
				Id = 30,
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 30,
			};
			var updatingClient = new Client
			{
				Id = 30,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new[] { address }

			};
			_context.Addresses.Add(address);
			_context.Clients.Add(updatingClient);
			_context.SaveChanges();
			//Act&Assert
			var addressU = new EditAddressDTO
			{
				Id = 30,
				Country = "Silesia",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
			};
			var id = 30;
			var updatedClient = new UpdateClientDTO
			{
				Name = "test1",
				//Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(id, updatedClient));
				Assert.Contains("email", ex.Message);			
		}
	}
}
