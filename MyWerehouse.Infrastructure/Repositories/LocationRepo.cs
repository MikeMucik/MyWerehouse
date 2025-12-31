using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class LocationRepo : ILocationRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public LocationRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public Location AddLocation(Location location)
		{
			_werehouseDbContext.Locations.Add(location);
			return location;
		}
		public void DeleteLocation(Location location)
		{
			_werehouseDbContext.Locations.Remove(location);
		}
		public async Task<Location> GetLocationByIdAsync(int locationId)
		{
			return await _werehouseDbContext.Locations.FindAsync(locationId);
		}
		public IQueryable<Location> GetAllAvailableLocations()
		{
			var locations = _werehouseDbContext.Locations
				.Where(l => l.Pallets.Count() == 0)
				.OrderBy(l => l.Id);
			return locations;
		}
		public async Task<Location> FindLocationAsync(int Bay, int Aisle, int Position, int Height)
		{
			var location = await _werehouseDbContext.Locations
				.FirstOrDefaultAsync(x => x.Bay == Bay &&
								x.Aisle == Aisle &&
								x.Position == Position &&
								x.Height == Height);
			return location;
		}
		public IEnumerable<Location> CreateListLocationForBayRangeAisle(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt)
		{
			var locations = new List<Location>();

			for (int i = startAisle; i <= endAisle; i++)
			{
				for (int j = 1; j <= amountPosition; j++)
				{
					for (int k = 1; k <= amountHeigt; k++)
					{
						locations.Add(new Location
						{
							Bay = bay,
							Aisle = i,
							Position = j,
							Height = k
						});
					}
				}
			}
			return locations;
		}

		public async Task<bool> ReceivingRampExistsAsync(int locationId)
		{
			if (await _werehouseDbContext.Locations.FindAsync(locationId) != null) { return true; } return false;
		}


		//public async Task AddManyLocationAsync(IEnumerable<Location> locations)
		//{
		//	await _werehouseDbContext.Locations.AddRangeAsync(locations);
		//}
	}
}
