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
		Task<int> AddLocation(LocationDTO locationDTO);
		Task CreateManySpots(int Bay, int Aisle, int Position, int Height);
		Task DeleteLocation(int id);		
		Task<LocationDTO> GetLocation(int id);
		Task<Location> FindLocationAsync(int Bay, int Aisle, int Position, int Heigt);
	}
}
