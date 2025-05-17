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
		void AddLocation (Location location);
		void UpdateLocation (Location location);
		void DeleteLocation (int locationId);
		Location GetLocationById (int locationId);
		IQueryable<Location> GetAllAvailableLocations ();

	}
}
