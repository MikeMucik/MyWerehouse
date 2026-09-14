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
			var addressU = new AddressDTO
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
				await _clientService.UpdateClientAsync(id,updatedClient, CancellationToken.None);
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
			var addressU = new AddressDTO
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
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(id, updatedClient, CancellationToken.None));
				Assert.Contains("Phone number is required.", ex.Message);
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
			var addressU = new AddressDTO
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
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(id, updatedClient, CancellationToken.None));
				Assert.Contains("Client email is required.", ex.Message);
		}

		[Fact]
		public async Task UpdateClient_ShouldAddNewAddressAndRemoveOldAddress_WhenOldAddressIsMissingFromRequest()
		{
			// Arrange
			var oldAddress = new Address
			{
				Id = 40,
				Country = "Poland",
				City = "Warsaw",
				Region = "Mazowieckie",
				Phone = 111111111,
				PostalCode = "00-001",
				StreetName = "Old Street",
				StreetNumber = "1",
				AdditionalEmail = "old@example.com",
				ClientId = 40,
			};
			var client = new Client
			{
				Id = 40,
				Name = "Client",
				Email = "client@example.com",
				Description = "Description",
				FullName = "Client Full Name",
				IsDeleted = false,
				Addresses = new List<Address> { oldAddress },
			};
			_context.Clients.Add(client);
			await _context.SaveChangesAsync();

			var newAddress = new AddressDTO
			{
				Country = "Germany",
				City = "Berlin",
				Region = "Berlin",
				Phone = 222222222,
				PostalCode = "10-115",
				StreetName = "New Street",
				StreetNumber = "2",
				AdditionalEmail = "new@example.com",
			};
			var updatedClient = new UpdateClientDTO
			{
				Name = "Updated Client",
				Email = "updated@example.com",
				Description = "Updated description",
				FullName = "Updated Client Full Name",
				Addresses = new[] { newAddress },
			};

			// Act
			var result = await _clientService.UpdateClientAsync(client.Id, updatedClient, CancellationToken.None);

			// Assert
			Assert.True(result.IsSuccess);
			_context.ChangeTracker.Clear();

			var savedClient = await _context.Clients
				.Include(c => c.Addresses)
				.SingleAsync(c => c.Id == client.Id);
			var savedAddress = Assert.Single(savedClient.Addresses);

			Assert.NotEqual(oldAddress.Id, savedAddress.Id);
			Assert.Equal(newAddress.Country, savedAddress.Country);
			Assert.Equal(newAddress.City, savedAddress.City);
			Assert.DoesNotContain(
				await _context.Addresses.ToListAsync(),
				a => a.Id == oldAddress.Id);
		}

		[Fact]
		public async Task UpdateClient_ShouldAddNewAddressAndUpdateOldAddress_WhenOldAddressIsIncludedInRequest()
		{
			// Arrange
			var oldAddress = new Address
			{
				Id = 50,
				Country = "Poland",
				City = "Warsaw",
				Region = "Mazowieckie",
				Phone = 333333333,
				PostalCode = "00-001",
				StreetName = "Old Street",
				StreetNumber = "1",
				AdditionalEmail = "old@example.com",
				ClientId = 50,
			};
			var client = new Client
			{
				Id = 50,
				Name = "Client",
				Email = "client@example.com",
				Description = "Description",
				FullName = "Client Full Name",
				IsDeleted = false,
				Addresses = new List<Address> { oldAddress },
			};
			_context.Clients.Add(client);
			await _context.SaveChangesAsync();

			var changedOldAddress = new AddressDTO
			{
				Id = oldAddress.Id,
				Country = "Czechia",
				City = "Prague",
				Region = "Prague",
				Phone = 444444444,
				PostalCode = "11-000",
				StreetName = "Updated Street",
				StreetNumber = "10",
				AdditionalEmail = "updated@example.com",
			};
			var newAddress = new AddressDTO
			{
				Country = "Germany",
				City = "Berlin",
				Region = "Berlin",
				Phone = 555555555,
				PostalCode = "10-115",
				StreetName = "New Street",
				StreetNumber = "2",
				AdditionalEmail = "new@example.com",
			};
			var updatedClient = new UpdateClientDTO
			{
				Name = "Updated Client",
				Email = "updated@example.com",
				Description = "Updated description",
				FullName = "Updated Client Full Name",
				Addresses = new[] { changedOldAddress, newAddress },
			};

			// Act
			var result = await _clientService.UpdateClientAsync(client.Id, updatedClient, CancellationToken.None);

			// Assert
			Assert.True(result.IsSuccess);
			_context.ChangeTracker.Clear();

			var savedClient = await _context.Clients
				.Include(c => c.Addresses)
				.SingleAsync(c => c.Id == client.Id);

			Assert.Equal(2, savedClient.Addresses.Count);
			var savedOldAddress = savedClient.Addresses.Single(a => a.Id == oldAddress.Id);
			var savedNewAddress = savedClient.Addresses.Single(a => a.Id != oldAddress.Id);

			Assert.Equal(changedOldAddress.Country, savedOldAddress.Country);
			Assert.Equal(changedOldAddress.City, savedOldAddress.City);
			Assert.Equal(changedOldAddress.AdditionalEmail, savedOldAddress.AdditionalEmail);
			Assert.Equal(newAddress.Country, savedNewAddress.Country);
			Assert.Equal(newAddress.City, savedNewAddress.City);
			Assert.NotEqual(0, savedNewAddress.Id);
		}
	}
}
