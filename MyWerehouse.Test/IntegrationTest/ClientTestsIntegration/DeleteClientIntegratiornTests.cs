using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using Xunit.Sdk;

namespace MyWerehouse.Test.IntegrationTest.ClientTestsIntegration
{
	public class DeleteClientIntegratiornTests : ClientIntegrationCommand
	{
		//var _receiptRepo = new ReceiptRepo();
		//_clientService = new ClientService(_clientRepo, _mapper , _receiptRepo);
		[Fact]
		public void HideClient_DeleteClient_ChangeNotActive()
		{
			//Arrange
			var clientId = 10;
			//Act
			_clientService.DeleteClient(clientId);
			//Assert
			var client = _context.Clients.FirstOrDefault(c=>c.Id == clientId);
			Assert.NotNull(client);
			Assert.True(client.IsDeleted);
		}
		[Fact]
		public void ReomveClient_DeleteClient_DeleteFromList()
		{
			//Arrange
			var clientId = 989;
			//Act
			_clientService.DeleteClient(clientId);
			//Assert
			var client = _context.Clients.FirstOrDefault(c=>c.Id==clientId);
			Assert.Null(client);
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
	}
}
