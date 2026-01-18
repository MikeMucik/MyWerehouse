using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface ILocationRepo
	{
		Location AddLocation(Location location);
		void DeleteLocation(Location location);
		Task<Location> GetLocationByIdAsync(int locationId);
		IQueryable<Location> GetAllAvailableLocations();
		Task<Location> FindLocationAsync(int Bay, int Aisle, int Position, int Heigt);
		IEnumerable<Location> CreateListLocationForBayRangeAisle(int Bay, int StartAisle, int EndAisle, int AmountPosition, int AmountHeigt);
		Task<bool> ReceivingRampExistsAsync(int locationId);
	}
}
