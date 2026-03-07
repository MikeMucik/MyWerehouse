using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface ILocationService
	{
		Task<AppResult<int>> AddLocationServiceAsync(LocationDTO locationDTO);
		Task<AppResult<Unit>> DeleteLocationServiceAsync(int id);
		Task<AppResult<LocationDTO>> GetLocationServiceAsync(int id);
		Task<AppResult<Location>> FindLocationAsync(int Bay, int Aisle, int Position, int Heigt);
		//Nie wiem po co to ale czuje że będzie potrzebne
		AppResult<List<LocationDTO>> PrepareLocationsAsync(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt);
		Task<AppResult<Unit>> CreateManyLocation(List<LocationDTO> locations);
	}
}
