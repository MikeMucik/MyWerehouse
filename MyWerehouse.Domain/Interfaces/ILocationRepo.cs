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
		Task<Location?> GetLocationByIdAsync(int locationId);
		IQueryable<Location> GetAllAvailableLocations();
		Task<Location?> FindLocationAsync(int Bay, int Aisle, int Position, int Height);
		IEnumerable<Location> CreateListLocationForBay(int Bay, int StartAisle, int EndAisle, int AmountPosition, int AmountHeight);
		Task<bool> ReceivingRampExistsAsync(int locationId);
		Task<bool> ExistsByCoordinatesAsync(int bay, int aisle, int position, int height);
	}
}
