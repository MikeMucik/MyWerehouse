using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.ViewModels.LocationModels
{
	public class LocationDTO : IMapFrom<Location>
	{
		public int Id { get; init; }
		public int Bay { get; init; }
		public int Aisle { get; init; }
		public int Position { get; init; }
		public int Height { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<LocationDTO, Location>()
				.ReverseMap();
		}
	}
}
