using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ClientTestsRepo
{
	[Collection("QuerryCollection")]
	public class ViewClientTests
	{
		private readonly ClientRepo _clientRepo;
		public ViewClientTests(QuerryTestFixture fixture) 
		{
			var _context = fixture.Context;
			_clientRepo = new ClientRepo(_context);
		}
		[Fact]
		public void ProperId_GetClientById_ReturnDataswithAddress()
		{
			//Arrange
			var id = 10;
			//Act
			var result = _clientRepo.GetClientById(id);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
			Assert.Equal("ClientTest", result.Name);
			Assert.Equal("FullNameTestAddress", result.FullName);
			Assert.Equal(2, result.Addresses.Count);			
		}
		[Fact]
		public void NotProperId_GetClientById_ReturnDataswithAddress()
		{
			//Arrange
			var id = -1;
			//Act
			var result = _clientRepo.GetClientById(id);
			//Assert
			Assert.Equal(null, result);
			//Assert.Equal("FullNameTestAddress1", result.Address.First(1). FullName);
		}
		[Fact]
		public void ShowAllClient_GetAllClient_ReturnList()
		{
			//Arrange
			//Act
			var result = _clientRepo.GetAllClients();
			//Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Count());
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
