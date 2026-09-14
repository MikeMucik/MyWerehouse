using FluentValidation;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Infrastructure.Persistence.ReadServices;
using MyWerehouse.Test.InMemoryDatabase.Common;
using MyWerehouse.Infrastructure.Common;

namespace MyWerehouse.Test.InMemoryDatabase.IntegrationTestService.ClientTestsIntegration
{

	[Collection("QueryCollectionInMemory")]
	public class ClientIntegrationServiceView : CommandTestBase
	{
		private readonly ClientRepo _clientRepo;
		private readonly ClientService _clientService;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IIssueRepo _issueRepo;

		private readonly IUnitOfWork _unitOfWork;
		private readonly IClientReadService _clientReadService;
		private readonly IValidator<AddClientDTO> _addClientValidator;
		private readonly IValidator<UpdateClientDTO> _updateClientValidator;
		private readonly IValidator<AddressDTO> _addAddressValidator;


		public ClientIntegrationServiceView(InMemoryDatabaseFixtureExecutive fixture)
		{
			var _context = fixture.Context;
			_clientRepo = new ClientRepo(_context);
			_receiptRepo = new ReceiptRepo(_context);
			_issueRepo = new IssueRepo(_context);

			_unitOfWork = new UnitOfWork(_context);
			_clientReadService = new ClientReadService(_context);
			_addAddressValidator = new AddressDTOValidation();
			
			_addClientValidator = new AddClientDTOValidation(_addAddressValidator);
			_updateClientValidator = new UpdateClientDTOValidation(_addAddressValidator);
			_clientService = new ClientService(_clientRepo, _receiptRepo, _issueRepo,_unitOfWork,_clientReadService, _addClientValidator, _updateClientValidator);
		}

		[Fact]
		public async Task DetailsOfClientAsync_ShouldReturnClientDetails_WhenDataValid()
		{
			//Arrange
			var clientId = 10;
			//Act
			var result = await _clientService.DetailsOfClientAsync(clientId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
				Assert.Equal("ClientTest", result.Result.Name);
			Assert.Equal("ClientDescription", result.Result.Description);
			Assert.Equal("ConutryTest", result.Result.Addresses.First().Country);
		}
		[Fact]
		public async Task DetailsOfClientAsync_ShouldReturnError_WhenWrongId()
		{
			//Arrange
			var clientId = 100;
			//Act
			var result = await _clientService.DetailsOfClientAsync(clientId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Null(result.Result);
			Assert.Contains($"Invalid client ID: {clientId}.", result.Error);
		}
		[Fact]
		public async Task DetailsOfClientAsync_ShouldReturnNull_WhenHideClient()
		{
			//Arrange
			var adrress = new Address
			{
				Country = "Poland",
				City = "Przeźmierowo",
				AdditionalEmail = "vef@fp.pl",
				Phone = 34554345,
				PostalCode = "12345",
				Region = "lubuskie",
				StreetName = "handlowa",
				StreetNumber = "123"
			};
			var client = new Client
			{
				Id = 100,
				IsDeleted = true,
				Name = "Piłka",
				FullName = "Piłka gumiana",
				Email = "dd@gmail.com",
				Description = "hjkofdiguhrnjkfigbuh",
				Addresses = new[] { adrress }
			};
			_context.Clients.Add(client);
			_context.SaveChanges();
			var clientId = 100;
			//Act
			var result = await _clientService.DetailsOfClientAsync(clientId, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Null(result.Result);
			Assert.Contains($"Invalid client ID: {clientId}.", result.Error);
		}
		[Fact]
		public async Task GetAllClientsAsync_ShouldReturnFirst3Client_WhenDataExist()
		{
			//Arrange&Act
			var result = await _clientService.GetAllClientsAsync(1, 3, CancellationToken.None);
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
			//Act
			var result = await _clientService.GetClientsByFilterAsync(1, 3, filter, CancellationToken.None);
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
			//Act
			var result = await _clientService.GetClientsByFilterAsync(1, 3, filter, CancellationToken.None);
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
			//Act
			var result = await _clientService.GetClientsByFilterAsync(1, 3, filter, CancellationToken.None);
			//Assert
			Assert.NotNull(result);
			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Result);
			Assert.Empty(result.Result.Items);
		}
	}
}
