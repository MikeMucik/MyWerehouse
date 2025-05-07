using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.UnitTestRepo.ClientTestsRepo
{
	public class UpdateClientTests
	{
		private readonly DbContextOptions<WerehouseDbContext> _context;
		public UpdateClientTests() : base()
		{
			_context = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("TestDatabase")
				.Options;
		}
		[Fact]
		public void UpdateProperData_UpdateClient_ShouldUpdateClient()
		{
			//Arrange
			var updatingClient = new Client
			{
				Id = 1,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "testFullNAME",
				IsDeleted = false,
			};
			using var arrangeContext = new WerehouseDbContext(_context);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
			//Act
			var updatedClient = new Client
			{
				Id = 1,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName="tESTFullName",
				IsDeleted = false,
				
			};
			using (var actContext = new WerehouseDbContext(_context))
			{
				var repo = new ClientRepo(actContext);
				repo.UpdateClient(updatedClient);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_context))
			{
				var result = assertContext.Clients.FirstOrDefault(c => c.Id == updatingClient.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedClient.Name, result.Name);
			}
		}
		[Fact]
		public void UpdateProperDataInner_UpdateClient_ShouldUpdateClient()
		{
			//Arrange
			var address = new Address
			{
				Id = 2,
				//FullName = "test",
				Country = "Poland",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 2,
			};
			var updatingClient = new Client
			{
				Id = 2,
				Name = "test",
				Email = "testEmail",
				Description = "test",
				FullName = "test",
				IsDeleted = false,
				Addresses = new[] { address }

			};
			using var arrangeContext = new WerehouseDbContext(_context);
			arrangeContext.Clients.Add(updatingClient);
			arrangeContext.SaveChanges();
			//Act
			var addressU = new Address
			{
				Id = 2,				
				Country = "Silesia",
				City = "test",
				Region = "test",
				Phone = 346456457,
				PostalCode = "test",
				StreetName = "test",
				StreetNumber = "test",
				ClientId = 2,
			};
			var updatedClient = new Client
			{
				Id = 2,
				Name = "test1",
				Email = "test1",
				Description = "test1",
				FullName = "test",
				IsDeleted = false,
				Addresses = new[] { addressU }
			};
			using (var actContext = new WerehouseDbContext(_context))
			{
				var repo = new ClientRepo(actContext);
				repo.UpdateClient(updatedClient);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_context))
			{
				var result = assertContext.Clients
					.Include(a=>a.Addresses).FirstOrDefault(c => c.Id == updatingClient.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedClient.Name, result.Name);
				Assert.Equal(updatedClient.Addresses.First().Country, result.Addresses.First().Country);
				Assert.Equal(updatedClient.Addresses.First().City, result.Addresses.First().City);
			}
		}
	}
}
