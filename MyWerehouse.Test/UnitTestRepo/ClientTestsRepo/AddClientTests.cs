using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.ClientTestsRepo
{
	public class AddClientTests : CommandTestBase
	{
		private readonly ClientRepo _clientRepo;
		public AddClientTests() : base()
		{
			_clientRepo = new ClientRepo(_context);
		}
		[Fact]
		public void AddPorperData_AddClient_ShouldAddToCollection()
		{
			//Arrange
			var client = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany"
			};
			//Act
			var result = _clientRepo.AddClient(client);
			//Assert
			Assert.NotNull(result);
			Assert.Equal(client.Id, result);
		}		
	}
}
