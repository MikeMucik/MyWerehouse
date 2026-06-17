using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Queries.GetPalletToEdit
{
	public class ShowPaletToEditDTO : IMapFrom<Pallet>
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; }
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; } = 0;
		public string UserId { get; init; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletDTO>();

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, ShowPaletToEditDTO>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(src => src.ProductsOnPallet));
		}
	}
}
