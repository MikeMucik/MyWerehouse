using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class EditPalletDTO : IMapFrom<Pallet>
	{
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public PalletStatus Status { get; set; } = 0;
		public string UserId { get; set; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();
		
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, EditPalletDTO>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(src => src.ProductsOnPallet));
		}
	}	
}