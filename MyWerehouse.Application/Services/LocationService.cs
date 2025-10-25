using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
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

		public async Task<int> AddLocationServiceAsync(LocationDTO locationDTO)
		{
			var location = _mapper.Map<Location>(locationDTO);
			var result = _locationRepo.AddLocation(location);
			await _werehouseDbContext.SaveChangesAsync();
			return result.Id;
		}
		public async Task DeleteLocationServiceAsync(int id)
		{
			//TODO warunek czy jest puste
			var isEmpty = _palletRepo.CheckOccupancyAsync(id);
			if (isEmpty != null)
			{
				throw new LocationException("Miejsce paletowe nie jest pustn nie można skasować");
			}
			var location = await _locationRepo.GetLocationByIdAsync(id);
			if (location == null)
			{
				throw new LocationException(id);
			}
			_locationRepo.DeleteLocation(location);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<LocationDTO> GetLocationServiceAsync(int id)
		{
			var location = await _locationRepo.GetLocationByIdAsync(id);
			var locationDTO = _mapper.Map<LocationDTO>(location);
			return locationDTO;
		}
		public async Task<Location> FindLocationAsync(int bay, int aisle, int position, int height)
		{
			var location = await _locationRepo.FindLocationAsync(bay, aisle, position, height);
			return location ?? throw new LocationException($"Nie ma lokalizacji o zadanych parametrach B:{bay}, A:{aisle}, P:{position}, H:{height}");
		}
		public List<LocationDTO> PrepareLocationsAsync(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt)
		{
			var list = new List<LocationDTO>();
			var locations = _locationRepo.CreateListLocationForBayRangeAisle(bay, startAisle, endAisle, amountPosition, amountHeigt);
			foreach (var location in locations)
			{
				var locationFrom = _mapper.Map<LocationDTO>(location);
				list.Add(locationFrom);
			}
			return list;
		}
		public async Task CreateManyLocation(List<LocationDTO> locations)
		{
			foreach (var locationDTO in locations.ToList())
			{
				var location = _mapper.Map<Location>(locationDTO);
				_locationRepo.AddLocation(location);
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
	}
}
