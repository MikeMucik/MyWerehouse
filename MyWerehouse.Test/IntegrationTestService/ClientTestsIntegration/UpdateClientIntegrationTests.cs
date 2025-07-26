using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.IntegrationTestService.ClientTestsIntegration
{
	public class UpdateClientIntegrationTests : ClientIntegrationCommand
	{	
		[Fact]
		public async Task ProperData_UpdateProductAsync_ChangeData()
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
			var updatedClient = new UpdateClientDTO
			{
				Id = 10,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};			
				await _clientService.UpdateClientAsync(updatedClient);			
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
		public async Task NotProperData_UpdateProductAsync_ThrowException()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
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
			arrangeContext.Addresses.Add(address);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
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
			var updatedClient = new UpdateClientDTO
			{
				Id = 20,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};					
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(updatedClient));
				Assert.Contains("tele", ex.Message);			
		}
		[Fact]
		public async Task NotProperDataEmail_UpdateProductAsync_ThrowException()
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
			var updatedClient = new UpdateClientDTO
			{
				Id = 30,
				Name = "test1",
				//Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(updatedClient));
				Assert.Contains("email", ex.Message);			
		}
	}
}
