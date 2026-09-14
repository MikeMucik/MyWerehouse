using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.IntegrationTestRepo.ClientTestsRepoSQLite
{
	[Collection("QueryCollectionInMemory")]
	public class ViewClientTests
	{
		private readonly ClientRepo _clientRepo;
		private readonly InMemoryDatabaseFixtureExecutive _fixture;
		public ViewClientTests(InMemoryDatabaseFixtureExecutive fixture)
		{
			_fixture = fixture;
			_clientRepo = new ClientRepo(_fixture.Context);
		}
		[Fact]
		public async Task ProperId_GetClientByIdAsync_ReturnDataswithAddress()
		{
			//Arrange
			var id = 10;
			//Act
			var result = await _clientRepo.GetClientByIdAsync(id, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal("ClientTest", result.Name);
			Assert.Equal("FullNameTestAddress", result.FullName);
			Assert.Equal(2, result.Addresses.Count);
		}
		[Fact]
		public async Task NotProperId_GetClientByIdAsync_NoReturnData()
		{
			//Arrange
			var id = -1;
			//Act
			var result = await _clientRepo.GetClientByIdAsync(id, CancellationToken.None);
			//Assert
			Assert.Null(result);
		}		
	}
}
