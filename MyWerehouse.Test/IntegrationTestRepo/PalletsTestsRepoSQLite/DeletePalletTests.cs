using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PalletsTestsRepoSQLite
{
	public class DeletePalletTests :TestBase
	{				
		//[Fact]
		//public void RemovePallet_DeletePallet_RemoveFromList()
		//{
		//	//Arrange
		//	var location = new Location
		//	{
		//		Bay = 1,
		//		Aisle = 1,
		//		Position = 1,
		//		Height = 1
		//	};
		//	DbContext.Locations.Add(location);
		//	var pallet1 = new Pallet
		//	{
		//		Id = "Q1000",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.ToIssue,				
		//	};
		//	var pallet2 = new Pallet
		//	{
		//		Id = "Q1011",
		//		DateReceived = DateTime.Now,
		//		LocationId = 1,
		//		Status = PalletStatus.Available,				
		//	};
		//	DbContext.Pallets.AddRange(pallet1, pallet2);
		//	DbContext.SaveChanges();
		//	var palletRepo = new PalletRepo(DbContext);
		//	//Act			
		//	palletRepo.DeletePallet(pallet1);
		//	DbContext.SaveChanges();
		//	//Assert
		//	var result = DbContext.Pallets.Find(pallet1.Id);
		//	Assert.Null(result);
		//}
	}
}
