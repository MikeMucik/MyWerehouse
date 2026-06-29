using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.InMemoryDatabase.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{

	[Collection("QueryCollectionInMemory")]
	public class ClientIntegrationServiceView : CommandTestBase
	{
		private readonly ClientRepo _clientRepo;
		private readonly ClientService _clientService;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IIssueRepo _issueRepo;

		public ClientIntegrationServiceView(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_clientRepo = new ClientRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_issueRepo = new IssueRepo(_context);
			_clientService = new ClientService(_clientRepo, _mapper, _receiptRepo, _issueRepo, _context);
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
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
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
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Null(result.Result);
			Assert.Contains($"Nieprawidłowy numer client {clientId}.", result.Error);
		}
		[Fact]
		public async Task GetAllClientsAsync_ShouldReturnFirst3Client_WhenDataExist()
		{
			//Arrange&Act
			var ct = CancellationToken.None;
			var result = await _clientService.GetAllClientsAsync(1, 3, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
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
			var result = await _clientService.GetClientsByFilterAsync(1, 3, filter, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
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
			var result = await _clientService.GetClientsByFilterAsync(1, 3, filter, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Single(result.Result.Items);
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
			var result = await _clientService.GetClientsByFilterAsync(1, 3, filter, ct);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Empty(result.Result.Items);
		}
	}
}

