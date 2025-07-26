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
		public async Task AddNewLocation_AddLocationAsync_AddedLoaction()
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
			var result = await _locationRepo.AddLocationAsync(location);
			await _context.SaveChangesAsync();
			//Arrange
			var existingLocation = _context.Locations
				.FirstOrDefault(l => l.Id == result);
			Assert.NotNull(result);
			Assert.Equal(location.Bay, existingLocation.Bay);
			Assert.Equal(location.Height, existingLocation.Height);
			Assert.Equal(location.Position, existingLocation.Position);
		}		
		[Fact]
		public async Task Removeloaction_DeleteLocationAsync_RemoveFromList()
		{
			//Arrange
			var location = new Location
			{
				Aisle = 10,
				Bay = 20,
				Height = 6,
				Position = 10
			};
			 _context.Locations.Add(location);
			_context.SaveChanges();
			var locationId = 1;
			//Act
			await _locationRepo.DeleteLocationAsync(locationId);
			await _context.SaveChangesAsync();
			//Assert
			var result = _context.Locations.Find(locationId);
			Assert.Null(result);		
		}
		//test do many
	}
}
