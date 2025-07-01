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
	public class AddClientTests : CommandTestBase
	{
		private readonly ClientRepo _clientRepo;
		public AddClientTests() : base()
		{
			_clientRepo = new ClientRepo(_context);
		}
		//[Fact]
		//public void AddProperData_AddClient_ShouldAddToCollection()
		//{
		//	//Arrange
		//	var client = new Client
		//	{				
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany"
		//	};
		//	//Act
		//	var result = _clientRepo.AddClient(client);
		//	//Assert			
		//	Assert.NotEqual(0, result);
		//	Assert.Equal(client.Id, result);
		//}
		//[Fact]
		//public async Task AddProperData_AddClientAsync_ShouldAddToCollection()
		//{
		//	//Arrange
		//	var client = new Client
		//	{
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany"
		//	};
		//	//Act
		//	var result = await _clientRepo.AddClientAsync(client);
		//	//Assert	
		//	Assert.NotEqual(0,result);		
		//	Assert.Equal(client.Id, result);
		//}
		//[Fact]
		//public void AddProperDataWithAddress_AddClient_ShouldAddToCollection()
		//{
		//	//Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var client = new Client
		//	{
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany",
		//		Addresses = [address]			
		//	};
		//	//Act
		//	var result = _clientRepo.AddClient(client);
		//	//Assert
		//	Assert.NotEqual(0, result);
		//	Assert.Equal(client.Id, result);
		//	var resultFull = _context.Clients.FirstOrDefault(c=>c.Id == client.Id);
		//	Assert.NotNull(resultFull);
		//	Assert.Single(resultFull.Addresses);
		//	Assert.Equal(resultFull.Addresses.First().StreetName, address.StreetName);
		//}
		//[Fact]
		//public async Task AddProperDataWithAddress_AddClientAsync_ShouldAddToCollection()
		//{
		//	//Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var client = new Client
		//	{
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany",
		//		Addresses = [address]

		//	};
		//	//Act
		//	var result =await _clientRepo.AddClientAsync(client);
		//	//Assert
		//	Assert.NotEqual(0,result);
		//	Assert.Equal(client.Id, result);
		//	var resultFull = _context.Clients
		//		.FirstOrDefault(c => c.Id == client.Id);
		//	Assert.NotNull(resultFull);
		//	Assert.Single(resultFull.Addresses);
		//	Assert.Equal(resultFull.Addresses.First().StreetName, address.StreetName);
		//}
		
	}
}
