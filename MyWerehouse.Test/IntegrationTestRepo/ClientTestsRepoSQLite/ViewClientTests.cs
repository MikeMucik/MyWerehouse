using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;
using Xunit;

namespace MyWerehouse.Test.UnitTestRepo.ClientTestsRepo
{
	[Collection("QueryCollection")]
	public class ViewClientTests
	{
		private readonly ClientRepo _clientRepo;
		private readonly QueryTestFixture _fixture;
		public ViewClientTests(QueryTestFixture fixture) 
		{
			_fixture= fixture;
			_clientRepo = new ClientRepo(_fixture.DbContext);
		}		
		[Fact]
		public async Task ProperId_GetClientByIdAsync_ReturnDataswithAddress()
		{
			//Arrange
			var id = 10;
			//Act
			var result =await _clientRepo.GetClientByIdAsync(id);
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
			var result =await _clientRepo.GetClientByIdAsync(id);
			//Assert
			Assert.Null(result);
		}
		[Fact]
		public void ShowAllClient_GetAllClient_ReturnList()
		{
			//Arrange
			//Act
			var result = _clientRepo.GetAllClients();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count());
		}
		[Fact]
		public void ShowClientsByPropertyAdressFullName_GetClients_ReturnList()
		{
			//Arrange
			var fullNameCompany = new ClientSearchFilter
			{
				FullName = "FullNameTestAddress"
			};
			//Act
			var result = _clientRepo.GetClients(fullNameCompany);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("ClientTest", result.First().Name);
		}
	}
}
