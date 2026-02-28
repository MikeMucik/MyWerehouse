using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Test.IntegrationTestService.ClientTestsIntegration
{
	public class DeleteClientIntegratiornTests : ClientIntegrationCommand
	{			
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
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var receipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 10,
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
		public async Task ReomveNotExistingClient_DeleteClientAsync_ThrowException()
		{
			//Arrange
			var clientId = 9891;
			//Act
			var e =await Assert.ThrowsAsync<DomainException>(() => _clientService.DeleteClientAsync(clientId));
			//Assert
			Assert.Equal($"Klient o numerze {clientId} nie istnieje.", e.Message);
		}
	}
}
