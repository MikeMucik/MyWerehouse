using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTestService.ClientTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewClientIntegrationTests(QuerryTestFixture fixture) : ClientIntegrationView(fixture)
	{		
		[Fact]
		public async Task ShowClientDetails_DetailsOfClientAsync_ReturnData()
		{
			//Arrange
			var clientId = 10;
			//Act
			var result =await _clientService.DetailsOfClientAsync(clientId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("ClientTest", result.Name);
			//Assert.Equal("TestDetails", result.Description);
			Assert.Equal("ConutryTest", result.Address.First().Country);
		}
		[Fact]
		public async Task ShowClientDetailsWrongId_DetailsOfClientAsync_ThrowException()
		{
			//Arrange
			var clientId = 100;
			//Act&Assert
			var result =await Assert.ThrowsAsync<ArgumentException>(() => _clientService.DetailsOfClientAsync(clientId));
			Assert.Contains("Nie ma takiego klienta", result.Message);
		}
		[Fact]
		public async Task ShowAllClient_First3_GetAllClientsAsync_ReturnList()
		{
			//Arrange&Act
			var result =await _clientService.GetAllClientsAsync(3, 1);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count);
		}
		[Fact]
		public async Task ShowClientByFilterName_GetClientsByFilterAsync_ReturnList()
		{
			//Arrange
			var filter = new ClientSearchFilter
			{
				Name = "Client"
			};
			//Act
			var result =await _clientService.GetClientsByFilterAsync(3, 1, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count);
		}
		[Fact]
		public async Task ShowClientByFilter_GetClientsByFilterAsync_ReturnList()
		{
			//Arrange
			var filter = new ClientSearchFilter
			{
				FullName = "FullNameTestAddress1"
			};
			//Act
			var result =await _clientService.GetClientsByFilterAsync(3, 1, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Count);
		}
		[Fact]
		public async Task ShowClientByFilterNotExist_GetClientsByFilterAsync_ReturnList()
		{
			//Arrange
			var filter = new ClientSearchFilter
			{
				Name = "Test1111"
			};
			//Act
			var result =await _clientService.GetClientsByFilterAsync(3, 1, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(0, result.Count);
		}
	}
}
