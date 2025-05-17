using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Test.Common;

namespace MyWerehouse.Test.UnitTestRepo.LocationTestsRepo
{
	public class AddUpdateDeleteLocationTests : CommandTestBase
	{
		private readonly LocationRepo _locationRepo;
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		public AddUpdateDeleteLocationTests() : base()
		{
			_locationRepo = new LocationRepo(_context);
			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase("TestDatabase")
				.Options;
		}
		[Fact]
		public void AddNewLocation_AddLocation_AddedLoaction()
		{
			//Arrange
			var location = new Location
			{
				Aisle = 10,
				Bay = 20,
				Height = 6,
				Position = 10
			};
			//Act
			_locationRepo.AddLocation(location);
			//Arrange
			var result = _context.Locations
				.FirstOrDefault(l => l.Aisle == 10);		
			Assert.NotNull(result);
			Assert.Equal(location.Bay, result.Bay);
			Assert.Equal(location.Height, result.Height);
			Assert.Equal(location.Position, result.Position);
		}
		[Fact]
		public void Removeloaction_DeleteLocation_RemoveFromList()
		{
			//Arrange
			var locationId = 1;
			//Act
			_locationRepo.DeleteLocation(locationId);
			//Assert
			var result = _context.Locations .Find(locationId);
			Assert.Null(result);
		}
		[Fact]
		public void UpdateLoaction_UpdateLocation_ChangeProperties()
		{
			//Arrange
			var updatingLocation = new Location
			{
				Id = 100,
				Aisle = 100,
				Bay = 100,
				Height = 1,
				Position = 1,				
			};
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			arrangeContext.Locations.Add(updatingLocation);
			arrangeContext.SaveChanges();
			//Act
			var updatedLocation = new Location
			{
				Id = 100,
				Aisle = 101,
				Bay = 101,
				Height = 5,
				Position =5,
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var repo = new LocationRepo(actContext);
				repo.UpdateLocation(updatedLocation);
			}
			//Assert
			using (var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Locations.Find(updatingLocation.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedLocation.Aisle, result.Aisle);
				Assert.Equal(updatedLocation.Bay, result.Bay);
				Assert.Equal(updatedLocation.Height, result.Height);
				Assert.Equal(updatedLocation.Position, result.Position);
				
			}
		}
	}
}
