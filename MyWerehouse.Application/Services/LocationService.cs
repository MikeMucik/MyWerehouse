using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Services
{
	public class LocationService(ILocationRepo locationRepo,
		IMapper mapper,
		IPalletRepo palletRepo,
		WerehouseDbContext werehouseDbContext) : ILocationService
	{
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly IMapper _mapper = mapper;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<int>> AddLocationServiceAsync(LocationDTO locationDTO)
		{
			if (await _locationRepo.ExistsByCoordinatesAsync(locationDTO.Bay, locationDTO.Aisle, locationDTO.Position, locationDTO.Height))
			{
				return AppResult<int>.Fail("A location with these coordinates already exists.",ErrorType.Conflict);
			}
			var location = _mapper.Map<Location>(locationDTO);

			var result = _locationRepo.AddLocation(location);

			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<int>.Success(result.Id, "Location added.");
		}
		public async Task<AppResult<Unit>> DeleteLocationServiceAsync(int id)
		{
			//warunek czy jest puste
			var isEmpty = await _palletRepo.CheckOccupancyAsync(id);
			if (isEmpty != null)
			{
				return AppResult<Unit>.Fail("The pallet location is not empty and cannot be deleted.", ErrorType.Conflict);
			}
			var location = await _locationRepo.GetLocationByIdAsync(id);
			if (location == null)
			{
				return AppResult<Unit>.Fail($"Location {id} was not found.");
			}
			_locationRepo.DeleteLocation(location);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value, "Operation completed successfully.");
		}
		public async Task<AppResult<LocationDTO>> GetLocationServiceAsync(int id)
		{
			var location = await _locationRepo.GetLocationByIdAsync(id);
			if (location == null) return AppResult<LocationDTO>.Fail("No location data to display.");
			var locationDTO = _mapper.Map<LocationDTO>(location);
			return AppResult<LocationDTO>.Success(locationDTO);
		}
		public async Task<AppResult<Location>> FindLocationAsync(int bay, int aisle, int position, int height)
		{
			var location = await _locationRepo.FindLocationAsync(bay, aisle, position, height);
			if (location is null) return AppResult<Location>.Fail($"No location matches the requested coordinates B:{bay}, A:{aisle}, P:{position}, H:{height}.");
			return AppResult<Location>.Success(location);
		}

		public AppResult<List<LocationDTO>> PrepareLocations(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt)
		{
			var list = new List<LocationDTO>();
			var locations = _locationRepo.CreateListLocationForBay(bay, startAisle, endAisle, amountPosition, amountHeigt);
			if (locations == null) return AppResult<List<LocationDTO>>.Fail("No location data to display.");

			foreach (var location in locations)
			{
				var locationFrom = _mapper.Map<LocationDTO>(location);
				list.Add(locationFrom);
			}
			return AppResult<List<LocationDTO>>.Success(list);
		}
		public async Task<AppResult<Unit>> CreateManyLocation(List<LocationDTO> locations)
		{
			foreach (var location in locations)
			{
				if (await _locationRepo.ExistsByCoordinatesAsync(location.Bay, location.Aisle, location.Position, location.Height))
				{
					return AppResult<Unit>.Fail($"A location with Bay = {location.Bay}, Aisle = {location.Aisle}, Position = {location.Position}, Height = {location.Height} already exists.", ErrorType.Conflict);
				}
			}
			foreach (var locationDTO in locations.ToList())
			{
				var location = _mapper.Map<Location>(locationDTO);
				_locationRepo.AddLocation(location);
			}
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value, "Locations added.");
		}
	}
}
