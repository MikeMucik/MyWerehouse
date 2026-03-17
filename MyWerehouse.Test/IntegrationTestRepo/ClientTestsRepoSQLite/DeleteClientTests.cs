using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ClientTestsRepoSQLite
{
	public class DeleteClientTests : TestBase
	{
		[Fact]
		public void SwichOffExistingClient_SwitchOffClient_ShouldChangeFlagOnIsDeleted()
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
			DbContext.Clients.Add(client);
			DbContext.Addresses.Add(address);
			DbContext.SaveChanges();
			var clientRepo = new ClientRepo(DbContext);
			//Act
			clientRepo.SwitchOffClient(client);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Clients.Find(client.Id);
			Assert.NotNull(result);
			Assert.True(result.IsDeleted);
		}
		[Fact]
		public void DeleteExistingClient_DeleteClient_ShouldRemoveClient()
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
			DbContext.Clients.Add(client);
			DbContext.Addresses.Add(address);
			DbContext.SaveChanges();
			var clientRepo = new ClientRepo(DbContext);
			//Act
			clientRepo.DeleteClient(client);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Clients.Find(client.Id);
			Assert.Null(result);
		}		
	}
}
