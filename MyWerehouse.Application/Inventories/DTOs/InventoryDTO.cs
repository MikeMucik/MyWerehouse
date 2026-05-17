using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Invetories.Models;

namespace MyWerehouse.Application.Inventories.DTOs
{
	public class InventoryDTO : IMapFrom<Inventory>
	{
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public DateTime LastUpdated { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Inventory, InventoryDTO>()
				.ForMember(dest => dest.ProductId, opt => opt.MapFrom(static src => src.ProductId))
				.ForMember(dest => dest.Quantity, opt => opt.MapFrom(static src => src.Quantity))
				.ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(static src => src.LastUpdated));
		}
	}
}
