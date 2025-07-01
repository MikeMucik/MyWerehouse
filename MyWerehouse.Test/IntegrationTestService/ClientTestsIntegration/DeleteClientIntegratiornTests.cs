using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using Xunit.Sdk;

namespace MyWerehouse.Test.IntegrationTest.ClientTestsIntegration
{
	public class DeleteClientIntegratiornTests : ClientIntegrationCommand
	{		
		[Fact]
		public void HideClient_DeleteClient_ChangeNotActive()
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
				Id = 10,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]

			};
			var receipt = new Receipt
			{
				Id= 10,
				ClientId =10,
				ReceiptDateTime = DateTime.Now,
				PerformedBy = "U1234"
			};
			_context.Receipts.Add(receipt);
			_context.Clients.Add(client);
			_context.SaveChanges();
			var clientId = 10;
			//Act
			_clientService.DeleteClient(clientId);
			//Assert
			var resultClient = _context.Clients.FirstOrDefault(c=>c.Id == clientId);
			Assert.NotNull(resultClient);
			Assert.True(resultClient.IsDeleted);
		}
		[Fact]
		public async Task HideClient_DeleteClientAsync_ChangeNotActive()
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
				Id = 10,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]

			};
			var receipt = new Receipt
			{
				Id = 10,
				ClientId = 10,
				ReceiptDateTime = DateTime.Now,
				PerformedBy = "U1234"
			};
			_context.Receipts.Add(receipt);
			_context.Clients.Add(client);
			_context.SaveChanges();
			var clientId = 10;
			//Act
			await _clientService.DeleteClientAsync(clientId);
			//Assert
			var resultClient = _context.Clients.FirstOrDefault(c => c.Id == clientId);
			Assert.NotNull(resultClient);
			Assert.True(resultClient.IsDeleted);
		}
		[Fact]
		public void ReomveClient_DeleteClient_DeleteFromList()
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
				Id = 10,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]

			};
			_context.Clients.Add(client);
			_context.SaveChanges();
			var clientId = 10;
			//Act
			_clientService.DeleteClient(clientId);
			//Assert
			var result = _context.Clients.FirstOrDefault(c=>c.Id==clientId);
			Assert.Null(result);
		}
		[Fact]
		public async Task ReomveClient_DeleteClientAsync_DeleteFromList()
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
				Id = 10,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]

			};
			_context.Clients.Add(client);
			_context.SaveChanges();
			var clientId = 10;
			//Act
			await _clientService.DeleteClientAsync(clientId);
			//Assert
			var result = _context.Clients.FirstOrDefault(c => c.Id == clientId);
			Assert.Null(result);
		}
		[Fact]
		public void ReomveNotExistingClient_DeleteClient_ThrowException()
		{
			//Arrange
			var clientId = 9891;
			//Act
			var e = Assert.Throws<ArgumentException>(() => _clientService.DeleteClient(clientId));
			//Assert
			Assert.Equal("Nie ma klienta o tym numerze", e.Message);
		}
		[Fact]
		public async Task ReomveNotExistingClient_DeleteClientAsync_ThrowException()
		{
			//Arrange
			var clientId = 9891;
			//Act
			var e =await Assert.ThrowsAsync<ArgumentException>(() => _clientService.DeleteClientAsync(clientId));
			//Assert
			Assert.Equal("Nie ma klienta o tym numerze", e.Message);
		}
	}
}
