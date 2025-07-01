using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using MyWerehouse.Domain.Models;
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
		//[Fact]
		//public void SwichOffExistingClient_SwitchOffClient_ShouldChangeFlagOnIsDeleted()
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
		//	_context.Clients.Add(client);
		//	_context.Addresses.Add(address);
		//	_context.SaveChanges();	
		//	var clientId = 1;
		//	//Act
		//	_clientRepo.SwitchOffClient(clientId);
		//	//Assert
		//	var result = _context.Clients.Find(clientId);
		//	Assert.NotNull(result);
		//	Assert.True(result.IsDeleted);
		//}
		//[Fact]
		//public async Task SwichOffExistingClient_SwitchOffClientAsync_ShouldChangeFlagOnIsDeleted()
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
		//	_context.Clients.Add(client);
		//	_context.Addresses.Add(address);
		//	_context.SaveChanges();
		//	var clientId = 1;
		//	//Act
		//	await _clientRepo.SwitchOffClientAsync(clientId);
		//	//Assert
		//	var result = _context.Clients.Find(clientId);
		//	Assert.NotNull(result);
		//	Assert.True(result.IsDeleted);
		//}
		//[Fact]
		//public void DeleteExistingClient_DeleteClient_ShouldRemoveClient()
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
		//	_context.Clients.Add(client);
		//	_context.Addresses.Add(address);
		//	_context.SaveChanges();
		//	var clientId = 1;
		//	//Act
		//	_clientRepo.DeleteClientById(clientId);
		//	//Assert
		//	var result = _context.Clients.Find(clientId);
		//	Assert.Null(result);
		//}
		//[Fact]
		//public async Task DeleteExistingClient_DeleteClientAsync_ShouldRemoveClient()
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
		//	_context.Clients.Add(client);
		//	_context.Addresses.Add(address);
		//	_context.SaveChanges();
		//	var clientId = 1;
		//	//Act
		//	await _clientRepo.DeleteClientByIdAsync(clientId);
		//	//Assert
		//	var result = _context.Clients.Find(clientId);
		//	Assert.Null(result);
		//}
	}
}
