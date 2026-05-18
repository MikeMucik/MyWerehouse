using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class HistoryPalletDTO : IMapFrom<HistoryPallet>
	{
		public int Id { get; set; }
		public string PalletId { get; set; }
		public int? SourceLocationId { get; set; }
		public string LocationSnapShotSource { get; set; }//
		public int? DestinationLocationId { get; set; }
		public string LocationSnapShotDestination { get; set; }//
		public ReasonForPallet Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string PerformedBy { get; set; } // opcjonalnie: user		
		public ICollection<HistoryPalletDetailDTO> HistoryPalletDetailsDTO { get; set; } = new List<HistoryPalletDetailDTO>();
		public DateTime MovementDate { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryPallet, HistoryPalletDTO>()
				.ForMember(dest => dest.HistoryPalletDetailsDTO, opt => opt.MapFrom(src => src.HistoryPalletDetails))
				.ForMember(dest=>dest.LocationSnapShotSource, opt=>opt.MapFrom(src => src.SourceLocationSnapShot))
				.ForMember(dest => dest.LocationSnapShotDestination, opt=>opt.MapFrom(src=>src.DestinationLocationSnapShot));
		}
	}
}
