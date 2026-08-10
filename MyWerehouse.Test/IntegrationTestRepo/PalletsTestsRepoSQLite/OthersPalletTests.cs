using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
		public async Task ReservePalletNumbersAsync_ShouldReturnFirstNumberAndAdvanceCounter()
		{
			//Arrange
			var counter = await DbContext.PalletNumberCounters
				.SingleAsync(x => x.Name == "Pallet");
			counter.NextNumber = 1012;
			await DbContext.SaveChangesAsync();
			var palletRepo = new PalletRepo(DbContext);

			//Act
			var result = await palletRepo.ReservePalletNumbersAsync(3);
			var updatedCounter = await DbContext.PalletNumberCounters
				.AsNoTracking()
				.SingleAsync(x => x.Name == "Pallet");

			//Assert
			Assert.Equal(1012, result);
			Assert.Equal(1015, updatedCounter.NextNumber);
		}		
	}
}
