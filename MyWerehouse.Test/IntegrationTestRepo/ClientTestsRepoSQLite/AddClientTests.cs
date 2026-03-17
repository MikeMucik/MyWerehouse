using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.UnitTestRepo.ClientTestsRepo
{
	public class AddClientTests : TestBase
	{
		[Fact]
		public void AddProperData_AddClient_ShouldAddToCollection()
		{
			//Arrange
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany"
			};
			var clientRepo = new ClientRepo(DbContext);
			//Act
			var result = clientRepo.AddClient(client);
			DbContext.SaveChanges();
			//Assert		
			var resultFull = DbContext.Clients.FirstOrDefault(c => c.Id == client.Id);
			Assert.NotNull(resultFull);
			Assert.NotEqual(0, resultFull.Id);
			Assert.Equal(client.Id, resultFull.Id);
		}		
		[Fact]
		public void AddProperDataWithAddress_AddClient_ShouldAddToCollection()
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var clientRepo = new ClientRepo(DbContext);
			//Act
			var result = clientRepo.AddClient(client);
			DbContext.SaveChanges();
			//Assert
			var resultFull = DbContext.Clients.FirstOrDefault(c => c.Id == client.Id);
			Assert.NotNull(resultFull);
			Assert.NotEqual(0, resultFull.Id);
			Assert.Equal(client.Id, resultFull.Id);

			Assert.Single(resultFull.Addresses);
			Assert.Equal(resultFull.Addresses.First().StreetName, address.StreetName);
		}
	}
}

