using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptById
{
	public class PalletForReceiptViewDTO : IMapFrom<Pallet>
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }
		public PalletStatus Status { get; init; } = 0;
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new List<ProductOnPalletDTO>();

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletForReceiptViewDTO>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(src => src.ProductsOnPallet));
		}
	}
}
