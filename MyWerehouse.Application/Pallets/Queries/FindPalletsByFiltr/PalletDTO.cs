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

namespace MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr
{
	public class PalletDTO : IMapFrom<Pallet>
	{
		public Guid Id { get; set; }
		public string PalletNumber { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public string LocationSnapShot { get; set; }
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new HashSet<ProductOnPalletDTO>();
		public ICollection<HistoryPalletDTO> PalletMovements { get; set; } = new List<HistoryPalletDTO>();
		public Guid? ReceiptId { get; set; }
		public int? ReceiptNumber { get; set; }     //
		public Guid? IssueId { get; set; }
		public int? IssueNumber { get; set; }//
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
