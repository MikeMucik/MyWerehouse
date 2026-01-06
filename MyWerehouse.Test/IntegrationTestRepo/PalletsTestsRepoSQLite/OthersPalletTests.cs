using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;
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
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.ToIssue,
				//ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
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
		[Fact]
		public void ChnageStatus_ClearPalletFromListIssue_ReturnNewStatus()
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
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.ToIssue,
				//ReceiptId = 10,
			};
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var palletRepo = new PalletRepo(DbContext);
			DbContext.SaveChanges();
			//Act
			palletRepo.ClearPalletFromListIssue(pallet2);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Pallets.Find(pallet2.Id);
			Assert.NotNull(result);
			Assert.Equal(PalletStatus.Available, result.Status);
		}
		[Fact]
		public void ChnageStatus_Pallet_ReturnNewStatus()
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
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.ToIssue,
				//ReceiptId = 10,
			};
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var palletRepo = new PalletRepo(DbContext);
			DbContext.SaveChanges();
			//Act
			palletRepo.ChangePalletStatus(pallet1.Id, PalletStatus.Damaged);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Pallets.Find(pallet1.Id);
			Assert.NotNull(result);
			Assert.Equal(PalletStatus.Damaged, result.Status);
		}
		[Fact]
		public void ChnageStatusToDefault_Pallet_ReturnNewStatus()
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
			var pallet1 = new Pallet
			{
				Id = "Q1010",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 10,
			};
			var pallet2 = new Pallet
			{
				Id = "Q1011",
				DateReceived = DateTime.Now,
				LocationId = 2,
				Status = PalletStatus.ToIssue,
				//ReceiptId = 10,
			};
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.SaveChanges();
			var palletRepo = new PalletRepo(DbContext);
			DbContext.SaveChanges();
			//Act
			palletRepo.ChangePalletStatus(pallet1.Id, (PalletStatus)9999);
			DbContext.SaveChanges();
			//Assert
			var result = DbContext.Pallets.Find(pallet1.Id);
			Assert.NotNull(result);
			Assert.Equal(PalletStatus.Available, result.Status);
		}
	}
}
