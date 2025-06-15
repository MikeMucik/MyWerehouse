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

namespace MyWerehouse.Test.IntegrationTest.ClientTestsIntegration
{
	public class UpdateClientIntegrationTests : ClientIntegrationCommand
	{
		[Fact]
		public void ProperData_UpdateProduct_ChangeData()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var address = new Address
			{
				Id = 1,
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 1,
			};
			var updatingClient = new Client
			{
				Id = 1,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new[] { address }

			};
			arrangeContext.Addresses.Add(address);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
			//Act
			var addressU = new AddressDTO
			{
				Id = 1,
				Country = "Silesia",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",				
			};
			var updatedClient = new AddClientDTO
			{
				Id = 1,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _clientValidator = new AddClientDTOValidation();
				var _addressValidator = new AddressDTOValidation();
				var _clientService = new ClientService(_clientRepo, _mapper, _addressValidator, _clientValidator);

				_clientService.UpdateClient(updatedClient);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Clients
					.Include(x => x.Addresses)
					.FirstOrDefault(x => x.Id == updatingClient.Id);

				Assert.NotNull(result);
				Assert.Equal(updatedClient.Name, result.Name);
				Assert.Equal(updatedClient.Addresses.First().Country, result.Addresses.First().Country);
				Assert.Equal(updatedClient.Addresses.First().City, result.Addresses.First().City);
			}
		}
		[Fact]
		public void NotProperData_UpdateProduct_ThrowException()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var address = new Address
			{
				Id = 2,
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 2,
			};
			var updatingClient = new Client
			{
				Id = 2,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new[] { address }

			};
			arrangeContext.Addresses.Add(address);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
			//Act&Assert
			var addressU = new AddressDTO
			{
				Id = 2,
				Country = "Silesia",
				City = "test",
				Region = "test",
				//Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",				
			};
			var updatedClient = new AddClientDTO
			{
				Id = 2,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _clientValidator = new AddClientDTOValidation();
				var _addressValidator = new AddressDTOValidation();
				var _clientService = new ClientService(_clientRepo, _mapper, _addressValidator, _clientValidator);

				var ex = Assert.Throws<FluentValidation.ValidationException>(()=> _clientService.UpdateClient(updatedClient));
				Assert.Contains("tele", ex.Message);
			}			
		}
		[Fact]
		public void NotProperDataEmail_UpdateProduct_ThrowException()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var address = new Address
			{
				Id = 3,
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 3,
			};
			var updatingClient = new Client
			{
				Id = 3,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new[] { address }

			};
			arrangeContext.Addresses.Add(address);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
			//Act&Assert
			var addressU = new AddressDTO
			{
				Id = 3,
				Country = "Silesia",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
			};
			var updatedClient = new AddClientDTO
			{
				Id = 3,
				Name = "test1",
				//Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU }
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _clientValidator = new AddClientDTOValidation();
				var _addressValidator = new AddressDTOValidation();
				var _clientService = new ClientService(_clientRepo, _mapper, _addressValidator, _clientValidator);

				var ex = Assert.Throws<FluentValidation.ValidationException>(() => _clientService.UpdateClient(updatedClient));
				Assert.Contains("email", ex.Message);
			}
		}
	}
}
