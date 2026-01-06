using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Application.ViewModels.LocationModels
{
	public class LocationDTO :IMapFrom<Location>
	{
		public int Id { get; set; }
		public int Bay { get; set; }
		public int Aisle { get; set; }
		public int Position { get; set; }
		public int Height { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<LocationDTO, Location>()
				.ReverseMap();
		}
	}
}
