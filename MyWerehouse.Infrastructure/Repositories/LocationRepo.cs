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

		public void AddLocation(Location location)
		{
			_werehouseDbContext.Locations.Add(location);
			_werehouseDbContext.SaveChanges();
		}
		public async Task AddLocationAsync(Location location)
		{
			await _werehouseDbContext.Locations.AddAsync(location);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public void DeleteLocation(int locationId)
		{
			var location = _werehouseDbContext.Locations.Find(locationId);
			if (location != null)
			{
				_werehouseDbContext.Locations.Remove(location);
				_werehouseDbContext.SaveChanges();
			}
		}
		public async Task DeleteLocationAsync(int locationId)
		{
			var location = await _werehouseDbContext.Locations.FindAsync(locationId);
			if (location != null)
			{
				_werehouseDbContext.Locations.Remove(location);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void UpdateLocation(Location location)
		{
			_werehouseDbContext.Attach(location);
			if (location.Aisle > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Aisle)).IsModified = true;
			}
			if (location.Bay > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Bay)).IsModified = true;
			}
			if (location.Position > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Position)).IsModified = true;
			}
			if (location.Height > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Height)).IsModified = true;
			}
			_werehouseDbContext.SaveChanges();
		}
		public async Task UpdateLocationAsync(Location location)
		{
			_werehouseDbContext.Attach(location);
			if (location.Aisle > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Aisle)).IsModified = true;
			}
			if (location.Bay > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Bay)).IsModified = true;
			}
			if (location.Position > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Position)).IsModified = true;
			}
			if (location.Height > 0)
			{
				_werehouseDbContext.Entry(location).Property(nameof(location.Height)).IsModified = true;
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		public Location? GetLocationById(int locationId)
		{
			return _werehouseDbContext.Locations.Find(locationId);
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
	}
}
