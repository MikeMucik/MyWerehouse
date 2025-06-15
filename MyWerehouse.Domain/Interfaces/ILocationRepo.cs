using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface ILocationRepo
	{
		void AddLocation(Location location);
		Task AddLocationAsync(Location location);
		void UpdateLocation(Location location);
		Task UpdateLocationAsync(Location location);
		void DeleteLocation(int locationId);
		Task DeleteLocationAsync(int locationId);
		Location? GetLocationById(int locationId);
		Task<Location?> GetLocationByIdAsync(int locationId);
		IQueryable<Location> GetAllAvailableLocations();
	}
}
