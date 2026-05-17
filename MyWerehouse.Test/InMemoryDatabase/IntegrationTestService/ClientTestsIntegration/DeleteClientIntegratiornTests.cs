using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{
	public class DeleteClientIntegratiornTests : ClientIntegrationCommand
	{
		[Fact]
		public async Task DeleteClientAsync_ShouldHideClient_WhenClientHasReceipt()
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
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = Receipt.CreateForSeed(receiptId1, 1, 10, "U1234",
			DateTime.UtcNow, ReceiptStatus.PhysicallyCompleted, 1);
			
			_context.Locations.Add(location);
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
		public async Task DeleteClientAsync_ShouldDeleteClient_WhenClientHasNoReceiptOrIssue()
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
		public async Task DeleteClientAsync_ShouldMessage_WhenClientNotExist()
		{
			//Arrange
			var clientId = 9891;
			//Act
			var result = await _clientService.DeleteClientAsync(clientId);
			//Assert
			Assert.Equal($"Klient o numerze {clientId} nie istnieje.", result.Error);
		}
	}
}
