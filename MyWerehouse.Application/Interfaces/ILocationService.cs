using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface ILocationService
	{
		Task<int> AddLocationServiceAsync(LocationDTO locationDTO);
		Task DeleteLocationServiceAsync(int id);
		Task<LocationDTO> GetLocationServiceAsync(int id);
		Task<Location> FindLocationAsync(int Bay, int Aisle, int Position, int Heigt);
		List<LocationDTO> PrepareLocationsAsync(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt);
		Task CreateManyLocation(List<LocationDTO> locations);
	}
}
