using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.Services
{
	public class LocationService(ILocationRepo locationRepo,
		ILocationReadService locationReadService,
		IPalletRepo palletRepo,		
		IUnitOfWork unitOfWork) : ILocationService
	{
		private readonly ILocationRepo _locationRepo = locationRepo;
		private readonly ILocationReadService _locationReadService = locationReadService;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;

		public async Task<AppResult<int>> AddLocationServiceAsync(LocationDTO locationDTO, CancellationToken ct)
		{
			if (await _locationRepo.ExistsByCoordinatesAsync(locationDTO.Bay, locationDTO.Aisle, locationDTO.Position, locationDTO.Height, ct))
			{
				return AppResult<int>.Fail("A location with these coordinates already exists.",ErrorType.Conflict);
			}
			var location = new Location
			{
				Bay = locationDTO.Bay,
				Aisle = locationDTO.Aisle,
				Height = locationDTO.Height,
				Position = locationDTO.Position,
			};
			var result = _locationRepo.AddLocation(location);

			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<int>.Success(result.Id, "Location added.");
		}
		public async Task<AppResult<Unit>> DeleteLocationServiceAsync(int id, CancellationToken ct)
		{
			//warunek czy jest puste
			var isEmpty = await _palletRepo.CheckOccupancyAsync(id, ct);
			if (isEmpty != null)
			{
				return AppResult<Unit>.Fail("The pallet location is not empty and cannot be deleted.", ErrorType.Conflict);
			}
			var location = await _locationRepo.GetLocationByIdAsync(id, ct);
			if (location == null)
			{
				return AppResult<Unit>.Fail($"Location {id} was not found.");
			}
			_locationRepo.DeleteLocation(location);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Operation completed successfully.");
		}
		public async Task<AppResult<LocationDTO>> GetLocationServiceAsync(int id, CancellationToken ct)
		{
			var location = await _locationReadService.GetLocationByIdAsync(id, ct);			
			if (location == null) return AppResult<LocationDTO>.Fail("No location data to display.");			
			return AppResult<LocationDTO>.Success(location);
		}
		public async Task<AppResult<int>> FindLocationIdAsync(int bay, int aisle, int position, int height, CancellationToken ct)
		{
			var location = await _locationReadService.FindLocationIdAsync(bay, aisle, position, height, ct);
			if (location is null) return AppResult<int>.Fail($"No location matches the requested coordinates B:{bay}, A:{aisle}, P:{position}, H:{height}.");
			return AppResult<int>.Success((int)location);
		}

		public AppResult<List<LocationDTO>> PrepareLocations(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt)
		{
			var list = new List<LocationDTO>();
			var locations = _locationRepo.CreateListLocationForBay(bay, startAisle, endAisle, amountPosition, amountHeigt);
			if (locations == null) return AppResult<List<LocationDTO>>.Fail("No location data to display.");

			foreach (var location in locations)
			{
				var locationFrom = new LocationDTO
				{
					Bay = location.Bay,
					Aisle = location.Aisle,
					Height = location.Height,
					Position = location.Position,
				};
				list.Add(locationFrom);
			}
			return AppResult<List<LocationDTO>>.Success(list);
		}
		public async Task<AppResult<Unit>> CreateManyLocation(List<LocationDTO> locations, CancellationToken ct)
		{
			foreach (var location in locations)
			{
				if (await _locationRepo.ExistsByCoordinatesAsync(location.Bay, location.Aisle, location.Position, location.Height, ct))
				{
					return AppResult<Unit>.Fail($"A location with Bay = {location.Bay}, Aisle = {location.Aisle}, Position = {location.Position}, Height = {location.Height} already exists.", ErrorType.Conflict);
				}
			}
			foreach (var locationDTO in locations.ToList())
			{
				var location = new Location
				{
					Bay = locationDTO.Bay,
					Aisle = locationDTO.Aisle,
					Height = locationDTO.Height,
					Position = locationDTO.Position,
				};
				_locationRepo.AddLocation(location);
			}
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Locations added.");
		}
	}
}
