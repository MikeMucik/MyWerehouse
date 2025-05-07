using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ClientTestsRepo
{
	public class DeleteClientTests : CommandTestBase
	{
		private readonly ClientRepo _clientRepo;
		public DeleteClientTests() : base()
		{
			_clientRepo = new ClientRepo(_context);
		}
		[Fact]
		public void SwichOffExistingClient_SwitchOffClient_ShouldChangeFlagOnIsDeleted()
		{
			//Arrange
			var numberClient = 10;
			//Act
			_clientRepo.SwitchOffClient(numberClient);
			//Assert
			var result = _context.Clients.Find(numberClient);
			Assert.True(result.IsDeleted);
		}
		[Fact]
		public void DeleteExistingClient_DeleteClient_ShouldRemoveClient()
		{
			//Arrange
			var numberClient = 11;
			//Act
			_clientRepo.DeleteClientById(numberClient);
			//Assert
			var result = _context.Clients.Find(numberClient);
			Assert.Null(result);
		}
	}
}
