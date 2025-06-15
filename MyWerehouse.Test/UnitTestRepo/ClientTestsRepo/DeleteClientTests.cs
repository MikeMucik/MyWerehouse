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
			var seededClientId = 10;
			//Act
			_clientRepo.SwitchOffClient(seededClientId);
			//Assert
			var result = _context.Clients.Find(seededClientId);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
		[Fact]
		public async Task SwichOffExistingClient_SwitchOffClientAsync_ShouldChangeFlagOnIsDeleted()
		{
			//Arrange
			var seededClientId = 10;
			//Act
			await _clientRepo.SwitchOffClientAsync(seededClientId);
			//Assert
			var result = _context.Clients.Find(seededClientId);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
		[Fact]
		public void DeleteExistingClient_DeleteClient_ShouldRemoveClient()
		{
			//Arrange
			var seededClientId = 11;
			//Act
			_clientRepo.DeleteClientById(seededClientId);
			//Assert
			var result = _context.Clients.Find(seededClientId);
			Assert.Null(result);
		}
		[Fact]
		public async Task DeleteExistingClient_DeleteClientAsync_ShouldRemoveClient()
		{
			//Arrange
			var seededClientId = 11;
			//Act
			await _clientRepo.DeleteClientByIdAsync(seededClientId);
			//Assert
			var result = _context.Clients.Find(seededClientId);
			Assert.Null(result);
		}
	}
}
