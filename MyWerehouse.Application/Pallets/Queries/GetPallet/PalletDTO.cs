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

namespace MyWerehouse.Application.Pallets.Queries.GetPallet
{
	public class PalletDTO : IMapFrom<Pallet>
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; }
		public DateTime DateReceived { get; init; }
		public int LocationId { get; init; }
		public string LocationSnapShot { get; init; }
		public PalletStatus Status { get; init; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new HashSet<ProductOnPalletDTO>();
		public ICollection<HistoryPalletDTO> PalletMovements { get; init; } = new List<HistoryPalletDTO>();
		public Guid? ReceiptId { get; init; }
		public int? ReceiptNumber { get; init; }     //
		public Guid? IssueId { get; init; }
		public int? IssueNumber { get; init; }//
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletDTO>()
				.ForMember(dest=>dest.PalletNumber, opt=>opt.MapFrom(src=>src.PalletNumber))
				.ForMember(dest=>dest.IssueNumber, opt=>opt.MapFrom(static src=> src.Issue.IssueNumber))
				.ForMember(dest=>dest.ProductsOnPallet, opt=>opt.MapFrom(static src=>src.ProductsOnPallet))
				.ForMember(dest => dest.LocationSnapShot, opt => opt.MapFrom(src => src.Location.Bay + " " +
				src.Location.Aisle + " " + src.Location.Position + " " + src.Location.Height));			
		}
	}
}
