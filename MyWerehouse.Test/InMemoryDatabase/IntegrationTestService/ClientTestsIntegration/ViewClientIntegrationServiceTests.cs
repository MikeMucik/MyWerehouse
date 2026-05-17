using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{

	[Collection("QueryCollectionInMemory")]
	public class ClientIntegrationServiceView : CommandTestBase
	{
		private readonly ClientRepo _clientRepo;
		private readonly ClientService _clientService;

		public ClientIntegrationServiceView(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_clientRepo = new ClientRepo(_context);
			_clientService = new ClientService(_clientRepo, _mapper);
		}
	
			[Fact]
			public async Task DetailsOfClientAsync_ShouldReturnClientDetails_WhenDataValid()
			{
				//Arrange
				var clientId = 10;
				//Act
				var result = await _clientService.DetailsOfClientAsync(clientId);
				//Assert
				Assert.NotNull(result);
				Assert.Equal("ClientTest", result.Result.Name);
				//Assert.Equal("TestDetails", result.Description);
				Assert.Equal("ConutryTest", result.Result.Address.First().Country);
			}
			[Fact]
			public async Task DetailsOfClientAsync_ShouldReturnError_WhenWrongId()
			{
				//Arrange
				var clientId = 100;
				//Act
				var result = await _clientService.DetailsOfClientAsync(clientId);
				//var result = await Assert.ThrowsAsync<NotFoundClientException>(() => _clientService.DetailsOfClientAsync(clientId));
				//Assert
				//Assert.Contains($"Nieprawidłowy numer client {clientId}.", result.Message);
				Assert.Contains($"Nieprawidłowy numer client {clientId}.", result.Error);
			}
			[Fact]
			public async Task GetAllClientsAsync_ShouldReturnFirst3Client_WhenDataExist()
			{
				//Arrange&Act
				var ct = CancellationToken.None;
				var result = await _clientService.GetAllClientsAsync(3, 1, ct);
				//Assert
				Assert.NotNull(result);
				Assert.Equal(3, result.Result.Items.Count);
			}
			[Fact]
			public async Task GetClientsByFilterAsync_ShouldReturnList_WhenFilterByNameExist()
			{
				//Arrange
				var filter = new ClientSearchFilter
				{
					Name = "Client"
				};
				var ct = CancellationToken.None;
				//Act
				var result = await _clientService.GetClientsByFilterAsync(3, 1, filter, ct);
				//Assert
				Assert.NotNull(result);
				Assert.Equal(3, result.Result.Items.Count);
			}
			[Fact]
			public async Task GetClientsByFilterAsync_ShouldReturnList_WhenFilterByFullNameExist()
		{
				//Arrange
				var filter = new ClientSearchFilter
				{
					FullName = "FullNameTestAddress1"
				};
				var ct = CancellationToken.None;
				//Act
				var result = await _clientService.GetClientsByFilterAsync(3, 1, filter, ct);
				//Assert
				Assert.NotNull(result);
				Assert.Equal(1, result.Result.Items.Count);
			}
			[Fact]
			public async Task GetClientsByFilterAsync_ShouldReturnEmptyList_WhenFilterByNameNotExist()
		{
				//Arrange
				var filter = new ClientSearchFilter
				{
					Name = "Test1111"
				};
				var ct = CancellationToken.None;
				//Act
				var result = await _clientService.GetClientsByFilterAsync(3, 1, filter, ct);
				//Assert
				Assert.NotNull(result);
				Assert.Equal(0, result.Result.Items.Count);
			}
		}
	}

