using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.IntegrationTest.ClientTestsIntegration
{
	[Collection("QuerryCollection")]
	public class ViewClientIntegrationTests(QuerryTestFixture fixture) : ClientIntegrationView(fixture)
	{
		[Fact]
		public void ShowClientDetails_DetailsOfClient_ReturnData()
		{
			//Arrange
			var clientId = 10;
			//Act
			var result = _clientService.DetailsOfClient(clientId);
			//Assert
			Assert.NotNull(result);
			Assert.Equal("ClientTest", result.Name);
			//Assert.Equal("TestDetails", result.Description);
			Assert.Equal("ConutryTest", result.Address.First().Country);
		}
		[Fact]
		public void ShowClientDetailsWrongId_DetailsOfClient_ThrowException()
		{
			//Arrange
			var clientId = 100;
			//Act&Assert
			var result =Assert.Throws<InvalidDataException>(()=> _clientService.DetailsOfClient(clientId));
			Assert.Contains("Nie ma takiego klienta", result.Message);
		}
		[Fact]
		public void ShowAllClient_First10_GetAllClients_ReturnList()
		{
			//Arrange&Act
			var result = _clientService.GetAllClients(3,1);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count);
		}
		[Fact]
		public void ShowClientByFilterName_GetClientsByFilter_ReturnList()
		{
			//Arrange
			var filter = new ClientSearchFilter
			{
				Name = "Test"
			};
			//Act
			var result = _clientService.GetClientsByFilter(3, 1, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(3, result.Count);
		}
		[Fact]
		public void ShowClientByFilter_GetClientsByFilter_ReturnList()
		{
			//Arrange
			var filter = new ClientSearchFilter
			{
				FullName = "FullNameTestAddress1"
			};
			//Act
			var result = _clientService.GetClientsByFilter(3, 1, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(1, result.Count);
		}
		[Fact]
		public void ShowClientByFilterNotExist_GetClientsByFilter_ReturnList()
		{
			//Arrange
			var filter = new ClientSearchFilter
			{
				Name = "Test1111"
			};
			//Act
			var result = _clientService.GetClientsByFilter(3, 1, filter);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(0, result.Count);
		}
	}
}
