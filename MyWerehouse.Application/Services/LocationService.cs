using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
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
		private readonly WerehouseDbContext _werehouseDbContext;

		public LocationService(ILocationRepo locationRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext
			)
		{
			_locationRepo = locationRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task<int> AddLocationServiceAsync(LocationDTO locationDTO)
		{
			var location = _mapper.Map<Location>(locationDTO);
			var result = await _locationRepo.AddLocationAsync(location);
			return result;
		}
		public async Task CreateManySpotsAsync(int Bay, int Aisle, int Position, int Height)
		{
			var location = new List<Location>();
			for (int i = 1; i <= Bay; i++)
			{
				for (int j = 0; j <= Aisle; j++)
				{
					for (int k = 1; k <= Position; k++)
					{
						for (int h = 1; h <= Height; h++)
						{
							location.Add(new Location
							{
								Bay = i,
								Aisle = j,
								Position = k,
								Height = h
							});
						}
					}
				}
			}
			await _locationRepo.AddManyLocationAsync(location);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task DeleteLocationServiceAsync(int id)
		{
			//TODO warunek czy jest puste
			var location = await _locationRepo.GetLocationByIdAsync(id);
			if (location == null)
			{
				throw new InvalidDataException($"Nie ma takiej lokalizacji o id {id}");
			}
			await _locationRepo.DeleteLocationAsync(id);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<Location> FindLocationAsync(int bay, int aisle, int position, int height)
		{
			var location = await _locationRepo.FindLocationAsync(bay, aisle, position, height);
			if (location == null)
			{
				throw new InvalidDataException($"Nie ma lokalizacji o zadanych parametrach B:{bay}, A:{aisle}, P:{position}, H:{height}");
			}
			return location;
		}
		public async Task<LocationDTO> GetLocationServiceAsync(int id)
		{
			var location = await _locationRepo.GetLocationByIdAsync(id);
			var locationDTO = _mapper.Map<LocationDTO>(location);
			return locationDTO;
		}
	}
}
