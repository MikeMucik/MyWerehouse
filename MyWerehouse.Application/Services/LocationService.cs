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
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class LocationService : ILocationService
	{
		private readonly ILocationRepo _locationRepo;
		private readonly IMapper _mapper;
		private readonly IPalletRepo _palletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;

		public LocationService(ILocationRepo locationRepo,
			IMapper mapper,
			IPalletRepo palletRepo,
			WerehouseDbContext werehouseDbContext)
		{
			_locationRepo = locationRepo;
			_mapper = mapper;
			_palletRepo = palletRepo;
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task<AppResult<int>> AddLocationServiceAsync(LocationDTO locationDTO)
		{
			var location = _mapper.Map<Location>(locationDTO);
			var result = _locationRepo.AddLocation(location);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<int>.Success(result.Id);
		}
		public async Task<AppResult<Unit>> DeleteLocationServiceAsync(int id)
		{
			//TODO warunek czy jest puste
			var isEmpty = _palletRepo.CheckOccupancyAsync(id);
			if (isEmpty != null)
			{
				return AppResult<Unit>.Fail("Miejsce paletowe nie jest puste nie można skasować", ErrorType.Conflict);
			}
			var location = await _locationRepo.GetLocationByIdAsync(id);
			if (location == null)
			{
				return AppResult<Unit>.Fail($"Lokalizacja o numerze {id} nie została znaleziona", ErrorType.NotFound);
			}
			_locationRepo.DeleteLocation(location);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value, "Operacja zakończyła się sukcesem");
		}
		public async Task<AppResult<LocationDTO>> GetLocationServiceAsync(int id)
		{
			var location = await _locationRepo.GetLocationByIdAsync(id);
			if (location == null) return AppResult<LocationDTO>.Fail("Brak elemntów do wyświetlenia", ErrorType.NotFound);
			var locationDTO = _mapper.Map<LocationDTO>(location);
			return AppResult<LocationDTO>.Success(locationDTO);
		}
		public async Task<AppResult<Location>> FindLocationAsync(int bay, int aisle, int position, int height)
		{
			var location = await _locationRepo.FindLocationAsync(bay, aisle, position, height);
			return AppResult<Location>.Success(location)
			?? AppResult<Location>.Fail($"Nie ma lokalizacji o zadanych parametrach B:{bay}, A:{aisle}, P:{position}, H:{height}");
		}

		//potrzebne do many 
		public AppResult<List<LocationDTO>> PrepareLocationsAsync(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt)
		{
			var list = new List<LocationDTO>();
			var locations = _locationRepo.CreateListLocationForBayRangeAisle(bay, startAisle, endAisle, amountPosition, amountHeigt);
			if (locations == null) return AppResult<List<LocationDTO>>.Fail("Brak elemntów do wyświetlenia", ErrorType.NotFound);

			foreach (var location in locations)
			{
				var locationFrom = _mapper.Map<LocationDTO>(location);
				list.Add(locationFrom);
			}
			return AppResult<List<LocationDTO>>.Success(list);
		}
		public async Task<AppResult<Unit>> CreateManyLocation(List<LocationDTO> locations)
		{
			foreach (var locationDTO in locations.ToList())
			{
				var location = _mapper.Map<Location>(locationDTO);
				_locationRepo.AddLocation(location);
			}
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
	}
}
