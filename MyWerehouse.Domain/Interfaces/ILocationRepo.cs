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
		Task<int> AddLocationAsync(Location location);				
		Task DeleteLocationAsync(int locationId);		
		Task<Location?> GetLocationByIdAsync(int locationId);
		IQueryable<Location> GetAllAvailableLocations();
		Task AddManyLocationAsync(IEnumerable<Location> locations);//test
		Task<Location> FindLocationAsync(int Bay, int Aisle, int Position, int Heigt);
	}
}
