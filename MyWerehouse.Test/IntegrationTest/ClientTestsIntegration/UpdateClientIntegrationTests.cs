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
				//Id = 200,
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
				Id = address.Id,
				Country = "Silesia",
				City = "test",
				Region = "testref",
				Phone = 98765432,
				PostalCode = "test66",
				StreetName = "test66",
				StreetNumber = "test66",
			};
			var addressU1 = new AddressDTO
			{
				//Id = 500,
				Country = "Mazovia",
				City = "test55",
				Region = "test55",
				Phone = 98765432,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
			};
			var updatedClient = new UpdateClientDTO
			{
				Id = 1,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				Addresses = new[] { addressU
					,addressU1
					}
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _addressValidator = new AddressDTOValidation();
				var _clientValidator = new UpdateClientDTOValidation(_addressValidator);
				var _clientService = new ClientService(_clientRepo, _mapper, _clientValidator);

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
				var updatedPhone = updatedClient.Addresses.First(c => c.Id == addressU.Id).Phone;
				var resultPhone = result.Addresses.First(c => c.Id == address.Id).Phone;
				Assert.Equal(updatedPhone, resultPhone);
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
			var updatedClient = new UpdateClientDTO
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
				var _addressValidator = new AddressDTOValidation();
				var _clientValidator = new UpdateClientDTOValidation(_addressValidator);
				var _clientService = new ClientService(_clientRepo, _mapper, null,
					//_addressValidator,
					_clientValidator);

				var ex = Assert.Throws<FluentValidation.ValidationException>(() => _clientService.UpdateClient(updatedClient));
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
			var updatedClient = new UpdateClientDTO
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
				var _addressValidator = new AddressDTOValidation();
				var _clientValidator = new UpdateClientDTOValidation(_addressValidator);
				var _clientService = new ClientService(_clientRepo, _mapper, null,
					//_addressValidator,
					_clientValidator);

				var ex = Assert.Throws<FluentValidation.ValidationException>(() => _clientService.UpdateClient(updatedClient));
				Assert.Contains("email", ex.Message);
			}
		}
		[Fact]
		public async Task ProperData_UpdateProductAsync_ChangeData()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
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
				Addresses = new[] { address }

			};
			arrangeContext.Addresses.Add(address);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
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
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _addressValidator = new AddressDTOValidation();
				var _clientValidator = new UpdateClientDTOValidation(_addressValidator);
				var _clientService = new ClientService(_clientRepo, _mapper, null,
					//_addressValidator,
					_clientValidator);

				await _clientService.UpdateClientAsync(updatedClient);
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
				Addresses = new[] { address }

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
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _addressValidator = new AddressDTOValidation();
				var _clientValidator = new UpdateClientDTOValidation(_addressValidator);
				var _clientService = new ClientService(_clientRepo, _mapper, null, _clientValidator);
				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(updatedClient));
				Assert.Contains("tele", ex.Message);
			}
		}
		[Fact]
		public async Task NotProperDataEmail_UpdateProductAsync_ThrowException()
		{
			//Arrange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
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
			arrangeContext.Addresses.Add(address);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
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
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _clientRepo = new ClientRepo(actContext);
				var _addressValidator = new AddressDTOValidation();
				var _clientValidator = new UpdateClientDTOValidation(_addressValidator);
				var _clientService = new ClientService(_clientRepo, _mapper, null,
					//_addressValidator,
					_clientValidator);

				var ex = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _clientService.UpdateClientAsync(updatedClient));
				Assert.Contains("email", ex.Message);
			}
		}
	}
}
