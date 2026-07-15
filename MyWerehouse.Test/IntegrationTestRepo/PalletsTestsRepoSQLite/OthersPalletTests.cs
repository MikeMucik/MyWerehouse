using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PalletsTestsRepoSQLite
{
	public class OthersPalletTests: TestBase
	{		
		[Fact]
		public async Task NextId_GetNextPalletIdAsync_ReturnNextId()
		{
			//Arrange
			var location1 = new Location
			{
				Bay = 1,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			var location2 = new Location
			{
				Bay = 2,
				Aisle = 1,
				Position = 1,
				Height = 1
			};
			DbContext.Locations.AddRange(location1, location2);
			var pallet1 = Pallet.CreateForTests("Q1010", DateTime.UtcNow, 1, PalletStatus.ToIssue, null, null);			
			var pallet2 = Pallet.CreateForTests("Q1011", DateTime.UtcNow, 2, PalletStatus.Available, null, null);			
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var palletRepo = new PalletRepo(DbContext);
			DbContext.SaveChanges();
			//Act
			var result =await palletRepo.GetNextPalletIdAsync();
			//Assert
			Assert.NotEmpty(result);
			Assert.Equal("Q1012", result);
		}		
	}
}
