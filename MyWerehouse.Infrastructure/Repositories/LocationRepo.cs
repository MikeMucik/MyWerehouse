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
		
		public async Task<int> AddLocationAsync(Location location)
		{
			await _werehouseDbContext.Locations.AddAsync(location);
			return location.Id;
		}		
		public async Task DeleteLocationAsync(int locationId)
		{
			var location = await _werehouseDbContext.Locations.FindAsync(locationId);
			if (location != null)
			{
				_werehouseDbContext.Locations.Remove(location);
				
			}
		}				
		public async Task<Location?> GetLocationByIdAsync(int locationId)
		{
			return await _werehouseDbContext.Locations.FindAsync(locationId);
		}
		public IQueryable<Location> GetAllAvailableLocations()
		{
			var locations = _werehouseDbContext.Locations
				.Where(l => l.Pallets.Count() == 0);
			return locations;
		}
		public async Task AddManyLocationAsync(IEnumerable<Location> locations)
		{
			await _werehouseDbContext.Locations.AddRangeAsync(locations);
		}
		public async Task<Location> FindLocationAsync(int Bay, int Aisle, int Position, int Heigt)
		{
			var location =await _werehouseDbContext.Locations
				.FirstOrDefaultAsync(x => x.Bay == Bay &&
								x.Aisle == Aisle && 
								x.Position == Position &&
								x.Height == Heigt);
			return location;
		}
	}
}
