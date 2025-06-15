using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.UnitTestRepo.PalletsTestsRepo
{
	public class UpdatePalletTests
	{
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public UpdatePalletTests()			
		{
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("TestDatabase")
				.Options;
		}
		[Fact]
		public void UpdateLocationAndStatus_UpdatePallet_ShouldChangeLocationAndStatus()
		{
			//Arrange
			var updatingPallet = new Pallet
			{
				Id = "Q10000",
				DateReceived = new DateTime(2020,1,1,0,0,0),
				LocationId = 1,
				Status = PalletStatus.Available,
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Pallets.Add(updatingPallet);
			arrangeContext.SaveChanges();
			//Act
			var updatedPallet = new Pallet 
			{
				Id = "Q10000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 2,
				Status = PalletStatus.OnHold,
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new PalletRepo(actContext);
				repo.UpdatePallet(updatedPallet);
			}

			//Assert
			using ( var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Pallets.Find(updatedPallet.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedPallet.LocationId, result.LocationId);
				Assert.Equal(updatedPallet.Status, result.Status);
			}
		}
		[Fact]
		public async Task UpdateLocationAndStatus_UpdatePalletAsync_ShouldChangeLocationAndStatus()
		{
			//Arrange
			var updatingPallet = new Pallet
			{
				Id = "Q11000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Pallets.Add(updatingPallet);
			arrangeContext.SaveChanges();
			//Act
			var updatedPallet = new Pallet
			{
				Id = "Q11000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 2,
				Status = PalletStatus.OnHold,
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new PalletRepo(actContext);
				await repo.UpdatePalletAsync(updatedPallet);
			}

			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Pallets.Find(updatedPallet.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedPallet.LocationId, result.LocationId);
				Assert.Equal(updatedPallet.Status, result.Status);
			}
		}
		[Fact]
		public void AddIssueId_UpdatePallet_ShouldChangeStatusAddIssueId()
		{
			//Arrange
			var updatingPallet = new Pallet
			{
				Id = "Q10001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Pallets.Add(updatingPallet);
			arrangeContext.SaveChanges();
			//Act
			var updatedPallet = new Pallet
			{
				Id = "Q10001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 2,
				Status = PalletStatus.OnHold,
				IssueId = 3
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new PalletRepo(actContext);
				repo.UpdatePallet(updatedPallet);
			}

			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Pallets.Find(updatingPallet.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedPallet.LocationId, result.LocationId);
				Assert.Equal(updatedPallet.Status, result.Status);
				Assert.Equal(updatedPallet.IssueId, result.IssueId);
			}
		}
		[Fact]
		public async Task AddIssueId_UpdatePalletAsync_ShouldChangeStatusAddIssueId()
		{
			//Arrange
			var updatingPallet = new Pallet
			{
				Id = "Q11001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Pallets.Add(updatingPallet);
			arrangeContext.SaveChanges();
			//Act
			var updatedPallet = new Pallet
			{
				Id = "Q11001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 2,
				Status = PalletStatus.OnHold,
				IssueId = 3
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new PalletRepo(actContext);
				await repo.UpdatePalletAsync(updatedPallet);
			}

			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Pallets.Find(updatingPallet.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedPallet.LocationId, result.LocationId);
				Assert.Equal(updatedPallet.Status, result.Status);
				Assert.Equal(updatedPallet.IssueId, result.IssueId);
			}
		}
	}
}
