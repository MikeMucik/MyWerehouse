using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class PalletDTOIssue : IMapFrom<Pallet>
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; }
		public int LocationId { get; init; }
		public string LocationSnapShot { get; init; }
		public PalletStatus Status { get; init; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new HashSet<ProductOnPalletDTO>();
		
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletDTOIssue>()
				.ForMember(dest => dest.PalletNumber, opt => opt.MapFrom(src => src.PalletNumber))
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(static src => src.ProductsOnPallet))
				.ForMember(dest => dest.LocationSnapShot, opt => opt.MapFrom(src => src.Location.Bay + " " +
				src.Location.Aisle + " " + src.Location.Position + " " + src.Location.Height));
		}
	}
}