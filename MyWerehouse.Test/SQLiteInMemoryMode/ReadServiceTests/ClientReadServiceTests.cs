using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Server.ServicesToInfrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class ClientReadServiceTests : TestBase
	{
		private readonly ClientReadService _clientReadService;

		public ClientReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			_clientReadService = new ClientReadService(DbContext);
		}

		[Fact]
		public async Task GetAllClientsAsync_ShouldReturnOnlyActiveClientsWithAddresses()
		{
			// Arrange
			var deletedClient = new Client
			{
				Id = 9001,
				Name = "DeletedClient",
				Email = "deleted@example.com",
				Description = "Deleted client used by the read-service test",
				FullName = "Deleted Client",
				IsDeleted = true
			};
			DbContext.Clients.Add(deletedClient);
			await DbContext.SaveChangesAsync();

			// Act
			var result = await _clientReadService.GetAllClientsAsync(
				pageNumber: 1,
				pageSize: 10,
				CancellationToken.None);

			// Assert
			Assert.Equal(3, result.TotalCount);
			Assert.DoesNotContain(result.Items, client => client.Id == deletedClient.Id);

			var clientWithAddresses = Assert.Single(
				result.Items,
				client => client.Id == 10);
			Assert.Equal(2, clientWithAddresses.Addresses.Count);
			Assert.Equal(
				["CityTest", "CityTest1"],
				clientWithAddresses.Addresses.Select(address => address.City));
		}

		[Fact]
		public async Task GetClientsByFilterAsync_ShouldFilterByAddressAndReturnMappedAddresses()
		{
			// Arrange
			var filter = new ClientSearchFilter
			{
				City = "CityTest1"
			};

			// Act
			var result = await _clientReadService.GetClientsByFilterAsync(
				pageNumber: 1,
				pageSize: 10,
				filter,
				CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalCount);
			var client = Assert.Single(result.Items);
			Assert.Equal(10, client.Id);
			Assert.Equal(2, client.Addresses.Count);
			Assert.Contains(client.Addresses, address => address.City == "CityTest1");
		}
	}
}
